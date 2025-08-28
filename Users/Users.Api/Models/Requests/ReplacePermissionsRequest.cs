namespace Users.Api.Controllers
{
	public partial class RolesController
	{
		public record ReplacePermissionsRequest(IEnumerable<Guid> PermissionIds);
	}
}
