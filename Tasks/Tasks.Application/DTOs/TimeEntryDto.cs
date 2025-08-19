using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tasks.Application.DTOs
{
	public class TimeEntryDto
	{
		public Guid Id { get; set; }
		public Guid UserId { get; set; }

		public DateTime StartedAt { get; set; }
		public DateTime EndedAt { get; set; }

		public string? Description { get; set; }

		public long DurationTicks { get; set; }

		public DateTime CreatedAt { get; set; }
	}
}
