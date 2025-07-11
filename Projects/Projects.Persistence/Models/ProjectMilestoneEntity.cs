using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projects.Persistence.Models
{
	public class ProjectMilestoneEntity
	{
		public Guid Id { get; set; }
		public Guid ProjectId { get; set; }
		public string Title { get; set; } = default!;
		public DateTime DueDate { get; set; }
		public bool IsCompleted { get; set; }
	}
}
