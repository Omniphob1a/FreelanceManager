using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tasks.Application.DTOs
{
	public class TaskListItemDto
	{
		public Guid Id { get; set; }
		public string Title { get; set; } = default!;
		public string Description { get; set; } = default!;
		public string AssigneeName { get; set; } = default!;
		public string ProjectName { get; set; } = default!;
		public DateTime DueDate { get; set; }
		public int Status { get; set; }
		public int Priority { get; set; } 
	}
}
