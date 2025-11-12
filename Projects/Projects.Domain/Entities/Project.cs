using Projects.Domain.Common;
using Projects.Domain.Entities;
using Projects.Domain.Enums;
using Projects.Domain.Events;
using Projects.Domain.ValueObjects;

namespace Projects.Domain.Entities;

public class Project : EntityBase
{
	public Guid Id { get; }
	public string Title { get; private set; }
	public string Description { get; private set; }
	public Guid OwnerId { get; }

	public Budget Budget { get; private set; }
	public Category Category { get; private set; }
	public ProjectStatus Status { get; private set; }
	public DateTime CreatedAt { get; }
	public DateTime? ExpiresAt { get; private set; }

	private readonly List<ProjectMilestone> _milestones = new();
	public IReadOnlyCollection<ProjectMilestone> Milestones => _milestones.AsReadOnly();

	private readonly List<ProjectAttachment> _attachments = new();
	public IReadOnlyCollection<ProjectAttachment> Attachments => _attachments.AsReadOnly();

	private readonly List<Tag> _tags = new();
	public IReadOnlyCollection<Tag> Tags => _tags.AsReadOnly();
	private readonly List<ProjectMember> _members = new();
	public IReadOnlyCollection<ProjectMember> Members => _members.AsReadOnly();

	public static Project CreateDraft(
		string title,
		string description,
		Guid ownerId,
		Budget budget,
		Category category,
		IEnumerable<Tag> tags)
	{
		Validate(title, description, ownerId, budget, category);

		var project = new Project(
			id: Guid.NewGuid(),
			title: title,
			description: description,
			ownerId: ownerId,
			budget: budget,
			category: category,
			tags: tags?.ToList() ?? new(),
			createdAt: DateTime.UtcNow);

		project.AddDomainEvent(new ProjectCreatedDomainEvent(project.Id, project.Title, project.OwnerId));
		return project;
	}

	public static Project Restore(
		Guid id,
		string title,
		string description,
		Guid ownerId,
		Budget budget,
		Category category,
		List<Tag> tags,
		List<ProjectMilestone> milestones,
		List<ProjectAttachment> attachments,
		List<ProjectMember> members,
		ProjectStatus status,
		DateTime? expiresAt,
		DateTime createdAt)
	{
		var project = new Project(id, title, description, ownerId, budget, category, tags, createdAt);

		project._milestones.AddRange(milestones ?? []);
		project._attachments.AddRange(attachments ?? []);
		project._members.AddRange(members ?? []);

		project.Status = status;
		project.ExpiresAt = expiresAt;

		return project;
	}

	private Project(
	   Guid id,
	   string title,
	   string description,
	   Guid ownerId,
	   Budget budget,
	   Category category,
	   List<Tag> tags,
	   DateTime createdAt) 
	{
		Id = id;
		Title = title;
		Description = description;
		OwnerId = ownerId;
		Budget = budget;
		Category = category;
		Status = ProjectStatus.Draft;
		CreatedAt = createdAt; 
		_tags = tags.Distinct().ToList();
	}

	public void UpdateDetails(
	   string title,
	   string description,
	   Budget budget,
	   Category category,
	   IEnumerable<Tag> tags)
	{
		if (Status != ProjectStatus.Draft)
			throw new InvalidOperationException("Only draft projects can be updated.");

		Validate(title, description, OwnerId, budget, category);

		Title = title;
		Description = description;
		Budget = budget;
		Category = category;

		_tags.Clear();
		_tags.AddRange(tags.Distinct());

		AddDomainEvent(new ProjectUpdatedDomainEvent(Id, Title, OwnerId));
	}

	public void Delete()
	{
		AddDomainEvent(new ProjectDeletedDomainEvent(Id));
	}

	public void Publish(DateTime expiresAt)
	{
		if (Status != ProjectStatus.Draft)
			throw new InvalidOperationException("Only draft projects can be published.");

		if (expiresAt <= DateTime.UtcNow)
			throw new ArgumentException("Expiration date must be in the future.");

		Status = ProjectStatus.Active;
		ExpiresAt = expiresAt;

		AddDomainEvent(new ProjectPublishedDomainEvent(Id, expiresAt));
	}

	public void Complete()
	{
		if (Status != ProjectStatus.Active)
			throw new InvalidOperationException("Only active projects can be completed.");

		Status = ProjectStatus.Completed;

		AddDomainEvent(new ProjectCompletedDomainEvent(Id));
	}

	public void Archive()
	{
		ExpiresAt = null;
		if (Status == ProjectStatus.Archived)
			throw new InvalidOperationException("Project is already archived.");

		Status = ProjectStatus.Archived;
		AddDomainEvent(new ProjectArchivedDomainEvent(Id));
	}

	public void AddMember(ProjectMember member)
	{
		if (member is null)
			throw new ArgumentNullException(nameof(member));

		if (_members.Any(m => m.UserId == member.UserId))
			throw new InvalidOperationException("User is already a member of this project.");

		_members.Add(member);
		AddDomainEvent(new ProjectMemberAddedDomainEvent(Id, member.Id, member.UserId, member.Role));
	}

	public void RemoveMember(Guid memberId)
	{
		var member = _members.FirstOrDefault(m => m.Id == memberId);
		if (member is null)
			throw new ArgumentException("Member not found.", nameof(memberId));

		_members.Remove(member);
		AddDomainEvent(new ProjectMemberRemovedDomainEvent(Id, member.Id, member.UserId));
	}

	public void ChangeMemberRole(Guid memberId, string newRole)
	{
		var member = _members.FirstOrDefault(m => m.Id == memberId);
		if (member is null)
			throw new ArgumentException("Member not found.", nameof(memberId));

		if (string.IsNullOrWhiteSpace(newRole))
			throw new ArgumentNullException(nameof(newRole));

		var updated = ProjectMember.Load(member.Id, member.UserId, newRole, member.ProjectId, member.AddedAt);

		_members.Remove(member);
		_members.Add(updated);

		AddDomainEvent(new ProjectMemberRoleChangedDomainEvent(Id, updated.Id, updated.UserId, newRole));
	}

	public void AddMilestone(ProjectMilestone milestone)
	{
		if (Status != ProjectStatus.Draft)
			throw new InvalidOperationException("Milestones can only be added in Draft.");

		_milestones.Add(milestone ?? throw new ArgumentNullException(nameof(milestone)));

		AddDomainEvent(new MilestoneAddedDomainEvent(Id, milestone));
	}

	public void CompleteMilestone(Guid milestoneId)
	{
		if (Status != ProjectStatus.Active)
			throw new InvalidOperationException("Only active projects can complete milestones.");

		var milestone = _milestones.FirstOrDefault(m => m.Id == milestoneId);
		if (milestone is null)
			throw new ArgumentException("Milestone not found.", nameof(milestoneId));

		milestone.MarkCompleted();
		AddDomainEvent(new MilestoneCompletedDomainEvent(Id, milestone.Id));
	}

	public void CheckEscalatedMilestones()
	{
		foreach (var milestone in _milestones.Where(m => !m.IsCompleted))
		{
			if (milestone.DueDate < DateTime.UtcNow && !milestone.IsEscalated)
			{
				milestone.MarkEscalated();
				Status = ProjectStatus.NeedsReview; 

				AddDomainEvent(new MilestoneEscalatedDomainEvent(Id, milestone.Id));
			}
		}
	}

	public void DeleteMilestone(ProjectMilestone milestone)
	{
		if (Status != ProjectStatus.Draft)
			throw new InvalidOperationException("Milestones can only be removed in Draft.");

		_milestones.Remove(milestone ?? throw new ArgumentNullException(nameof(milestone)));

		AddDomainEvent(new MilestoneRemovedDomainEvent(Id, milestone));
	}

	public void AddAttachment(ProjectAttachment attachment)
	{
		_attachments.Add(attachment ?? throw new ArgumentNullException(nameof(attachment)));

		AddDomainEvent(new AttachmentAddedDomainEvent(Id, attachment));
	}

	public void DeleteAttachment(ProjectAttachment attachment)
	{
		_attachments.Remove(attachment ?? throw new ArgumentNullException(nameof(attachment)));

		AddDomainEvent(new AttachmentRemovedDomainEvent(Id, attachment));
	}

	public void AddTag(Tag tag)
	{
		if (tag is null)
			throw new ArgumentNullException(nameof(tag));

		if (_tags.Contains(tag))
			return;

		_tags.Add(tag);

		AddDomainEvent(new TagsAddedDomainEvent(Id, tag));
	}

	public void DeleteTag(Tag tag)
	{
		if (tag is null)
			throw new ArgumentNullException(nameof(tag));

		_tags.Remove(tag);

		AddDomainEvent(new TagsDeletedDomainEvent(Id, tag));
	}
	private static void Validate(
		string title,
		string description,
		Guid ownerId,
		Budget budget,
		Category category)
	{
		if (string.IsNullOrWhiteSpace(title))
			throw new ArgumentException("Title is required.");

		if (string.IsNullOrWhiteSpace(description))
			throw new ArgumentException("Description is required.");

		if (ownerId == Guid.Empty)
			throw new ArgumentException("Owner ID is required.");

		if (budget is null)
			throw new ArgumentNullException(nameof(budget));

		if (category is null)
			throw new ArgumentNullException(nameof(category));
	}

	public bool IsExpired()
	{
		return ExpiresAt.HasValue && ExpiresAt.Value <= DateTime.UtcNow;
	}
}
