using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tasks.Application.DTOs
{
	public class ProjectMemberReadDto
	{
		public Guid Id { get; set; }
		public Guid ProjectId { get; set; }
		public string Role { get; set; } = default!;
		public DateTime AddedAt { get; set; }
		public PublicUserDto? User { get; set; } = default!;
	}
}
