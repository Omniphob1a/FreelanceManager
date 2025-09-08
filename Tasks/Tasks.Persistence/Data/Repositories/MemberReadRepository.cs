using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tasks.Application.DTOs;
using Tasks.Application.Interfaces;
using Tasks.Persistence.Models.ReadModels; 

namespace Tasks.Persistence.Data.Repositories
{
	public class MemberReadRepository : IMemberReadRepository
	{
		private readonly ProjectTasksDbContext _context;
		private readonly ILogger<MemberReadRepository> _logger;

		public MemberReadRepository(ProjectTasksDbContext context, ILogger<MemberReadRepository> logger)
		{
			_context = context;
			_logger = logger;
		}

		public Task<bool> ExistsAsync(Guid projectId, Guid userId, CancellationToken ct)
		{
			_logger.LogInformation("Checking if member exists by projectId {ProjectId} and userId {UserId}", projectId, userId);

			if (projectId == Guid.Empty)
				throw new ArgumentNullException(nameof(projectId), "ProjectId cannot be empty");

			if (userId == Guid.Empty)
				throw new ArgumentNullException(nameof(userId), "UserId cannot be empty");

			try
			{
				var isExists = _context.Set<MemberReadModel>()
					.AnyAsync(m => m.ProjectId == projectId && m.UserId == userId, ct);

				return isExists;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Unexpected error while checking member existence");
				throw;
			}
		}

		public async Task<List<MemberDto>> GetByProjectAsync(Guid projectId, CancellationToken ct)
		{
			_logger.LogInformation("Trying to get members by project id {ProjectId}", projectId);

			if (projectId == Guid.Empty)
				throw new ArgumentNullException(nameof(projectId), "ProjectId cannot be empty");

			try
			{
				var members = await _context.Set<MemberReadModel>()
					.Where(m => m.ProjectId == projectId)
					.OrderBy(m => m.AddedAt)
					.Select(m => new MemberDto
					{
						Id = m.Id,
						UserId = m.UserId,
						ProjectId = m.ProjectId,
						Role = m.Role,
						AddedAt = m.AddedAt
					})
					.ToListAsync(ct);

				return members;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to get members by project id {ProjectId}", projectId);
				throw;
			}
		}
	}
}
