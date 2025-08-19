using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projects.Application.DTOs
{
	public class ProjectMilestoneDto
	{
		public Guid Id { get; set; }
		public string Title { get; set; } = default!;
		public DateTime DueDate { get; set; }
		public bool IsCompleted { get; set; }
		public bool IsEscalated { get; set; }
	}
}
