using System;
using System.Collections.Generic;

namespace Tasks.Application.DTOs
{
	public class ProjectTaskDto
	{
		public Guid Id { get; set; }
		public Guid ProjectId { get; set; }

		public string Title { get; set; } = string.Empty;
		public string? Description { get; set; }

		public Guid? AssigneeId { get; set; }
		public Guid ReporterId { get; set; }

		public int Status { get; set; }          
		public int Priority { get; set; }         

		public long? TimeEstimatedTicks { get; set; }
		public long? TimeSpentTicks { get; set; }

		public DateTime? DueDate { get; set; }

		public bool IsBillable { get; set; }

		public DateTime CreatedAt { get; set; }
		public DateTime UpdatedAt { get; set; }
		public Guid CreatedBy { get; set; }

		public List<TimeEntryDto> TimeEntries { get; set; } = new();
		public List<CommentDto> Comments { get; set; } = new();
	}
}
