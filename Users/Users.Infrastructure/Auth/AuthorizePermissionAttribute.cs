using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Users.Domain.ValueObjects;


namespace Users.Infrastructure.Auth
{
	public class AuthorizePermissionAttribute : AuthorizeAttribute
	{
		private const string PREFIX = "Permission:";

		public AuthorizePermissionAttribute(string permissionName)
		{
			Policy = $"{PREFIX}{permissionName}";
		}
	}

}
