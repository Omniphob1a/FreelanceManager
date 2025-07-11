using Projects.Domain.Entities.ProjectService.Domain.Entities;
using Projects.Domain.Enums;
using Projects.Domain.ValueObjects;

namespace Projects.Domain.Entities
{
	public class Project
	{
		public Guid Id { get; }
		public string Title { get; private set; }
		public string Description { get; private set; }
		public Guid OwnerId { get; }
		public Budget Budget { get; private set; }
		public string Category { get; private set; }
		public ProjectStatus Status { get; private set; }
		public DateTime CreatedAt { get; }
		public DateTime? ExpiresAt { get; private set; }

		private readonly List<ProjectMilestone> _milestones = new();
		public IReadOnlyCollection<ProjectMilestone> Milestones => _milestones.AsReadOnly();

		private readonly List<ProjectAttachment> _attachments = new();
		public IReadOnlyCollection<ProjectAttachment> Attachments => _attachments.AsReadOnly();

		private readonly List<string> _tags = new();
		public IReadOnlyCollection<string> Tags => _tags.AsReadOnly();

		private Project(
			Guid id,
			string title,
			string description,
			Guid ownerId,
			Budget budget,
			string category,
			IEnumerable<string> tags,
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

			if (tags is not null)
				_tags = tags.Where(t => !string.IsNullOrWhiteSpace(t)).Distinct().ToList();
		}

		public static Project CreateDraft(
			string title,
			string description,
			Guid ownerId,
			Budget budget,
			string category,
			IEnumerable<string> tags)
		{
			if (string.IsNullOrWhiteSpace(title))
				throw new ArgumentException("Title is required.", nameof(title));

			if (string.IsNullOrWhiteSpace(description))
				throw new ArgumentException("Description is required.", nameof(description));

			if (ownerId == Guid.Empty)
				throw new ArgumentException("OwnerId is required.", nameof(ownerId));

			if (budget is null)
				throw new ArgumentNullException(nameof(budget), "Budget is required.");

			if (string.IsNullOrWhiteSpace(category))
				throw new ArgumentException("Category is required.", nameof(category));

			return new Project(
				id: Guid.NewGuid(),
				title: title,
				description: description,
				ownerId: ownerId,
				budget: budget,
				category: category,
				tags: tags,
				createdAt: DateTime.UtcNow);
		}

		public void Publish(DateTime expiresAt)
		{
			if (Status != ProjectStatus.Draft)
				throw new InvalidOperationException("Only draft projects can be published.");

			if (expiresAt <= DateTime.UtcNow)
				throw new ArgumentException("Expiration date must be in the future.");

			ExpiresAt = expiresAt;
			Status = ProjectStatus.Active;
		}

		public void Complete()
		{
			if (Status != ProjectStatus.Active)
				throw new InvalidOperationException("Only active projects can be completed.");

			Status = ProjectStatus.Completed;
		}

		public void Archive()
		{
			if (Status == ProjectStatus.Archived)
				return;

			Status = ProjectStatus.Archived;
		}

		public void AddMilestone(ProjectMilestone milestone)
		{
			if (Status != ProjectStatus.Draft)
				throw new InvalidOperationException("Can only add milestones in Draft state.");

			_milestones.Add(milestone ?? throw new ArgumentNullException(nameof(milestone)));
		}

		public void AddAttachment(ProjectAttachment attachment)
		{
			_attachments.Add(attachment ?? throw new ArgumentNullException(nameof(attachment)));
		}

		public void ChangeBudget(Budget newBudget)
		{
			Budget = newBudget ?? throw new ArgumentNullException(nameof(newBudget));
		}

		public void UpdateTitle(string title)
		{
			if (Status != ProjectStatus.Draft)
				throw new InvalidOperationException("Title can only be changed in Draft status.");

			Title = title ?? throw new ArgumentNullException(nameof(title));
		}

		public void AddTag(string tag)
		{
			if (!string.IsNullOrWhiteSpace(tag) && !_tags.Contains(tag))
				_tags.Add(tag);
		}
	}
}
