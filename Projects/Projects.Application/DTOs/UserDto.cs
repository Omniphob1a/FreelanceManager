using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projects.Application.DTOs
{
	public class UserDto
	{
		public Guid Id { get; set; }
		public string Login { get; set; } = default!;
		public string Name { get; set; } = default!;
		public int Gender { get; set; }
		public DateTime? Birthday { get; set; }
		public string Email { get; set; } = default!;
		public bool Admin { get; set; }
		public DateTime CreatedAt { get; set; }
		public DateTime? ModifiedOn { get; set; }
		public DateTime? RevokedOn { get; set; }
		public IEnumerable<string> Roles { get; set; } = Array.Empty<string>();
		public IEnumerable<string> Permissions { get; set; } = Array.Empty<string>();
	}
}
