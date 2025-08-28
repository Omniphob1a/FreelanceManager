namespace Users.Api.Controllers
{
	public partial class RolesController
	{
		public record CreateRoleRequest(string Name, IEnumerable<Guid>? PermissionIds);
	}
}
