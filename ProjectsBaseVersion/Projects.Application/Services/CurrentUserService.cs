using Microsoft.AspNetCore.Http;
using Projects.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Projects.Application.Services
{
	public class CurrentUserService : ICurrentUserService
	{
		private readonly IHttpContextAccessor _context;

		public CurrentUserService(IHttpContextAccessor context)
		{
			_context = context;
		}

		public Guid UserId
		{
			get
			{
				var userId = _context.HttpContext?.User?
					.FindFirst(ClaimTypes.NameIdentifier)?.Value
					?? throw new UnauthorizedAccessException("User ID not found in token");

				return Guid.TryParse(userId, out var guid)
					? guid
					: throw new UnauthorizedAccessException("Invalid User ID format");
			}
		}
	}
}
