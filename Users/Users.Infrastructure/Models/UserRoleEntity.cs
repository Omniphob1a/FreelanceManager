using Users.Domain.Entities;
using Users.Infrastructure.Models;

namespace Users.Infrastructure.Models;
public class UserRoleEntity
{
	public Guid UserId { get; set; }
	public UserData User { get; set; } = null!;

	public Guid RoleId { get; set; }
	public RoleEntity Role { get; set; } = null!;
}