using Users.Domain.Entities;
using Users.Infrastructure.Models;

namespace Users.Infrastructure.Models;
public class RolePermissionEntity
{
	public Guid RoleId { get; set; }
	public RoleEntity Role { get; set; } = null!;

	public Guid PermissionId { get; set; }
	public PermissionEntity Permission { get; set; } = null!;
}
