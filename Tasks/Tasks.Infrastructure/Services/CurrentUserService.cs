using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Tasks.Application.Interfaces;

namespace Tasks.Infrastructure.Services
{
	public class CurrentUserService : ICurrentUserService
	{
		private readonly IHttpContextAccessor _httpContextAccessor;
		private readonly ILogger<CurrentUserService> _logger;

		public CurrentUserService(
			IHttpContextAccessor httpContextAccessor,
			ILogger<CurrentUserService> logger)
		{
			_httpContextAccessor = httpContextAccessor;
			_logger = logger;
		}

		public Guid UserId
		{
			get
			{
				try
				{
					var user = _httpContextAccessor.HttpContext?.User;
					var isAuthenticated = user?.Identity?.IsAuthenticated ?? false;

					_logger.LogInformation("User.Identity.IsAuthenticated: {IsAuthenticated}", isAuthenticated);

					var allClaims = user?.Claims?.Select(c => $"{c.Type} = {c.Value}").ToList() ?? new List<string>();
					_logger.LogInformation("All Claims: {Claims}", string.Join(", ", allClaims));

					if (!isAuthenticated || user == null)
					{
						_logger.LogWarning("User is not authenticated or user context is null.");
						throw new UnauthorizedAccessException("User is not authenticated.");
					}

					var userIdClaim =
						user.FindFirst("nameid")?.Value ??
						user.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
						user.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;

					_logger.LogInformation("Extracted User ID claim value: {UserIdClaim}", userIdClaim ?? "NULL");

					if (string.IsNullOrWhiteSpace(userIdClaim))
					{
						_logger.LogWarning("User ID not found in JWT token claims");
						throw new UnauthorizedAccessException("User ID not found in token");
					}

					if (!Guid.TryParse(userIdClaim, out var guid))
					{
						_logger.LogWarning("Invalid User ID format in JWT token: {UserId}", userIdClaim);
						throw new UnauthorizedAccessException("Invalid User ID format");
					}

					return guid;
				}
				catch (Exception ex) when (!(ex is UnauthorizedAccessException))
				{
					_logger.LogError(ex, "Error retrieving user ID from JWT token");
					throw new UnauthorizedAccessException("Error retrieving user information");
				}
			}
		}
	}
}
