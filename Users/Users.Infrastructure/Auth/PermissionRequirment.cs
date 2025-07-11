using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
namespace Users.Infrastructure.Auth
{
	public class PermissionRequirement : IAuthorizationRequirement
	{
		public string PermissionName { get; }
		public PermissionRequirement(string permissionName) =>
			PermissionName = permissionName;
	}
}
