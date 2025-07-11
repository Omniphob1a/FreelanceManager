using Users.Domain.Entities;

namespace Users.Infrastructure.Models
{
	public class PermissionEntity
	{
		public Guid Id { get; set; }
		public string Name { get; set; }
		public ICollection<RolePermissionEntity> RolePermissions { get; set; } = new List<RolePermissionEntity>();
	}
}
