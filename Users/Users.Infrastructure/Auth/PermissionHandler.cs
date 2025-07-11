using Microsoft.AspNetCore.Authorization;

namespace Users.Infrastructure.Auth
{
	public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
	{
		protected override Task HandleRequirementAsync(
			AuthorizationHandlerContext context,
			PermissionRequirement requirement)
		{
			var hasClaim = context.User
				.Claims
				.Where(c => c.Type == "permission")
				.Select(c => c.Value)
				.Contains(requirement.PermissionName);

			if (hasClaim)
				context.Succeed(requirement);

			return Task.CompletedTask;
		}
	}

}
