namespace Projects.Domain.Entities
{
	public class ProjectMilestone
	{
		public Guid Id { get; }
		public string Title { get; private set; }
		public DateTime DueDate { get; private set; }
		public bool IsCompleted { get; private set; }
		public bool IsEscalated { get; private set; }
		public Guid ProjectId { get; }

		public ProjectMilestone(string title, DateTime dueDate, Guid projectId)
		{
			Id = Guid.NewGuid();
			Title = title ?? throw new ArgumentNullException(nameof(title));

			if (dueDate < DateTime.UtcNow)
				throw new ArgumentException("Due date must be in the future for new milestones.");

			DueDate = dueDate;
			IsCompleted = false;
			IsEscalated = false;
			ProjectId = projectId;
		}

		private ProjectMilestone(
			Guid id,
			string title,
			DateTime dueDate,
			bool isCompleted,
			bool isEscalated,
			Guid projectId)
		{
			Id = id;
			Title = title ?? throw new ArgumentNullException(nameof(title));
			DueDate = dueDate; 
			IsCompleted = isCompleted;
			IsEscalated = isEscalated;
			ProjectId = projectId;
		}

		public static ProjectMilestone Load(
			Guid id,
			string title,
			DateTime dueDate,
			bool isCompleted,
			bool isEscalated,
			Guid projectId)
		{
			return new ProjectMilestone(
				id,
				title,
				dueDate,
				isCompleted,
				isEscalated,
				projectId);
		}

		public void MarkCompleted() => IsCompleted = true;
		public void MarkEscalated()
		{
			if (!IsEscalated)
			{
				IsEscalated = true;
			}
		}

		public void Reschedule(DateTime newDueDate)
		{
			if (newDueDate <= DateTime.UtcNow)
				throw new ArgumentException("New due date must be in the future.");

			DueDate = newDueDate;
		}
	}
}