namespace Users.Infrastructure.Models
{
	public class RoleEntity
	{
		public Guid Id { get; set; }
		public string Name { get; set; } = null!;
		public ICollection<UserRoleEntity> UserRoles { get; set; } = new List<UserRoleEntity>();
		public ICollection<RolePermissionEntity> RolePermissions { get; set; } = new List<RolePermissionEntity>();
	}
}
