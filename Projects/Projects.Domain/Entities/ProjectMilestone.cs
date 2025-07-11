using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projects.Domain.Entities
{
	namespace ProjectService.Domain.Entities
	{
		public class ProjectMilestone
		{
			public Guid Id { get; }
			public string Title { get; private set; }
			public DateTime DueDate { get; private set; }
			public bool IsCompleted { get; private set; }
			public Guid ProjectId { get; private set; }

			public ProjectMilestone(string title, DateTime dueDate, Guid projectId)
			{
				Id = Guid.NewGuid();
				Title = title ?? throw new ArgumentNullException(nameof(title));
				DueDate = dueDate >= DateTime.UtcNow
					? dueDate
					: throw new ArgumentException("Due date must be in the future.");

				IsCompleted = false;
				ProjectId = projectId;
			}

			public void MarkCompleted()
			{
				IsCompleted = true;
			}

			public void Reschedule(DateTime newDueDate)
			{
				if (newDueDate <= DateTime.UtcNow)
					throw new ArgumentException("New due date must be in the future.");

				DueDate = newDueDate;
			}

			public static ProjectMilestone Load(string title, DateTime dueDate, bool isCompleted, Guid projectId)
			{
				var milestone = new ProjectMilestone(title, dueDate, projectId);
				if (isCompleted)
				{
					milestone.MarkCompleted();
				}
				return milestone;
			}
		}
	}

}
