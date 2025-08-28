using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Users.Application.DTOs
{
	public class RoleDto
	{
		public Guid Id { get; set; }
		public string Name { get; set; } = null!;
		public IList<Guid> PermissionIds { get; set; } = new List<Guid>();
	}
}
