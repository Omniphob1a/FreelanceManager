using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tasks.Infrastructure.DTOs
{
	public class ProjectMemberDto
	{
		public Guid Id { get; set; }
		public Guid UserId { get; set; }
		public string Role { get; set; } = default!;
		public Guid ProjectId { get; set; }
		public DateTime AddedAt { get; set; }
	}
}
