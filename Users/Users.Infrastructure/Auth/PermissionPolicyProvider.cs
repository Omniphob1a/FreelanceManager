using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace Users.Infrastructure.Auth
{
	public class PermissionPolicyProvider : IAuthorizationPolicyProvider
	{
		const string PREFIX = "Permission:";
		private readonly DefaultAuthorizationPolicyProvider _fallback;

		public PermissionPolicyProvider(IOptions<AuthorizationOptions> options)
		{
			_fallback = new DefaultAuthorizationPolicyProvider(options);
		}

		public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
		{
			if (policyName.StartsWith(PREFIX))
			{
				var permName = policyName[PREFIX.Length..];
				var policy = new AuthorizationPolicyBuilder()
					.AddRequirements(new PermissionRequirement(permName))
					.Build();
				return Task.FromResult<AuthorizationPolicy?>(policy);
			}
			return _fallback.GetPolicyAsync(policyName);
		}

		public Task<AuthorizationPolicy?> GetDefaultPolicyAsync() =>
			_fallback.GetDefaultPolicyAsync();

		public Task<AuthorizationPolicy?> GetFallbackPolicyAsync() =>
			_fallback.GetFallbackPolicyAsync();
	}

}
