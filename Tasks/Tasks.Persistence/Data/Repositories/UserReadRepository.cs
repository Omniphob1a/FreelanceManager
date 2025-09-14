using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tasks.Application.DTOs;
using Tasks.Application.Interfaces;
using Tasks.Persistence.Data;
using Tasks.Persistence.Models.ReadModels;

namespace Tasks.Persistence.Data.Repositories
{
	public class UserReadRepository : IUserReadRepository
	{
		private readonly ProjectTasksDbContext _context;
		private readonly ILogger<UserReadRepository> _logger;

		public UserReadRepository(ProjectTasksDbContext context, ILogger<UserReadRepository> logger)
		{
			_context = context;
			_logger = logger;
		}

		public async Task<PublicUserDto?> GetByIdAsync(Guid userId, CancellationToken ct)
		{
			_logger.LogInformation("Fetching public user data by userId {UserId}", userId);

			if (userId == Guid.Empty)
				throw new ArgumentNullException(nameof(userId), "UserId cannot be empty");

			try
			{
				return await _context.Set<UserReadModel>()
					.Where(u => u.Id == userId)
					.Select(u => new PublicUserDto
					{
						Id = u.Id,
						Name = u.Name,
						Login = u.Login,
						Gender = u.Gender,
						Birthday = u.Birthday
					})
					.FirstOrDefaultAsync(ct);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to fetch public user data for userId {UserId}", userId);
				throw;
			}
		}

		public async Task<List<PublicUserDto>> GetByIdsAsync(IEnumerable<Guid> userIds, CancellationToken ct)
		{
			_logger.LogInformation("Fetching public user data by userIds");

			if (!userIds.Any())
				throw new ArgumentNullException(nameof(userIds), "UserIds cannot be empty");

			try
			{
				return await _context.Users
					.AsNoTracking()
					.Where(u => userIds.Contains(u.Id))
					.Select(u => new PublicUserDto
					{
						Id = u.Id,
						Name = u.Name,
						Login = u.Login,
						Gender = u.Gender,
						Birthday = u.Birthday
					})
					.ToListAsync(ct);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to fetch public user data by userIds");
				throw;
			}

		}

		public async Task<List<PublicUserDto>> GetAllAsync(CancellationToken ct)
		{
			_logger.LogInformation("Fetching all public users");

			try
			{
				return await _context.Set<UserReadModel>()
					.OrderBy(u => u.Name)
					.Select(u => new PublicUserDto
					{
						Id = u.Id,
						Name = u.Name,
						Login = u.Login,
						Gender = u.Gender,
						Birthday = u.Birthday
					})
					.ToListAsync(ct);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to fetch all public users");
				throw;
			}
		}

		public Task<bool> ExistsAsync(Guid userId, CancellationToken ct)
		{
			_logger.LogInformation("Checking if user exists by userId {UserId}", userId);

			if (userId == Guid.Empty)
				throw new ArgumentNullException(nameof(userId), "UserId cannot be empty");

			try
			{
				return _context.Set<UserReadModel>()
					.AnyAsync(u => u.Id == userId, ct);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Unexpected error while checking user existence");
				throw;
			}
		}
	}
}
