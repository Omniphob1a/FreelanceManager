using Projects.Domain.Common;
using Projects.Domain.Entities.ProjectService.Domain.Entities;
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
			Guid.NewGuid(),
			title,
			description,
			ownerId,
			budget,
			category,
			tags?.ToList() ?? new());

		project.AddDomainEvent(new ProjectCreatedDomainEvent(project.Id));   

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
	ProjectStatus status,
	DateTime? expiresAt)
	{
		var project = new Project(id, title, description, ownerId, budget, category, tags);

		foreach (var milestone in milestones)
			project.AddMilestone(milestone);

		foreach (var attachment in attachments)
			project.AddAttachment(attachment);

		switch (status)
		{
			case ProjectStatus.Completed:
				project.Complete();
				break;
			case ProjectStatus.Archived:
				project.Archive();
				break;
			case ProjectStatus.Active when expiresAt.HasValue:
				project.Publish(expiresAt.Value);
				break;
		}

		return project;
	}

	private Project(
		Guid id,
		string title,
		string description,
		Guid ownerId,
		Budget budget,
		Category category,
		List<Tag> tags)
	{
		Id = id;
		Title = title;
		Description = description;
		OwnerId = ownerId;
		Budget = budget;
		Category = category;
		Status = ProjectStatus.Draft;
		CreatedAt = DateTime.UtcNow;
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

		AddDomainEvent(new ProjectUpdatedDomainEvent(Id));           
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
		if (Status == ProjectStatus.Archived)
			throw new InvalidOperationException("Project is already archived.");

		Status = ProjectStatus.Archived;

		AddDomainEvent(new ProjectArchivedDomainEvent(Id));                
	}

	public void AddMilestone(ProjectMilestone milestone)
	{
		if (Status != ProjectStatus.Draft)
			throw new InvalidOperationException("Milestones can only be added in Draft.");

		_milestones.Add(milestone ?? throw new ArgumentNullException(nameof(milestone)));

		AddDomainEvent(new MilestoneAddedDomainEvent(Id, milestone));      
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
}
