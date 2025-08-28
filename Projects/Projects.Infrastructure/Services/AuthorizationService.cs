using Microsoft.AspNetCore.Http;
using Projects.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tasks.Infrastructure.Services
{
	public class AuthorizationService : IAuthorizationService
	{
		private readonly IHttpContextAccessor _httpContext;

		public AuthorizationService(IHttpContextAccessor httpContext)
		{
			_httpContext = httpContext;
		}

		public string? GetAccessToken()
		{
			var header = _httpContext.HttpContext.Request.Headers["Authorization"].ToString();

			if (string.IsNullOrWhiteSpace(header))
				return null;

			return header.Replace("Bearer ", "");
		}
	}
}
