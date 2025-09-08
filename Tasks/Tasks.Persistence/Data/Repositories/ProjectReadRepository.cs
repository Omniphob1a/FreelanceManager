using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Tasks.Application.DTOs;
using Tasks.Application.Interfaces;
using Tasks.Persistence.Data;
using Tasks.Persistence.Models.ReadModels;

namespace Tasks.Persistence.Data.Repositories
{
	public class ProjectReadRepository : IProjectReadRepository
	{
		private readonly ProjectTasksDbContext _context;
		private readonly ILogger<ProjectReadRepository> _logger;

		public ProjectReadRepository(ProjectTasksDbContext context, ILogger<ProjectReadRepository> logger)
		{
			_context = context;
			_logger = logger;
		}

		public Task<bool> ExistsAsync(Guid projectId, CancellationToken ct)
		{
			_logger.LogInformation("Checking project existence by id {ProjectId}", projectId);

			if (projectId == Guid.Empty)
			{
				_logger.LogWarning("ProjectId cannot be empty");
				throw new ArgumentNullException(nameof(projectId));
			}

			try
			{
				return _context.Set<ProjectReadModel>()
					.AsNoTracking()
					.AnyAsync(p => p.Id == projectId, ct);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Unexpected error while checking project {ProjectId} existence", projectId);
				throw;
			}
		}

		public async Task<ProjectDto?> GetByIdAsync(Guid projectId, CancellationToken ct)
		{
			_logger.LogInformation("Getting project by id {ProjectId}", projectId);

			if (projectId == Guid.Empty)
			{
				_logger.LogWarning("ProjectId cannot be empty");
				throw new ArgumentNullException(nameof(projectId));
			}

			try
			{
				return await _context.Set<ProjectReadModel>()
					.AsNoTracking()
					.Where(p => p.Id == projectId)
					.Select(p => new ProjectDto
					{
						Id = p.Id,
						Title = p.Title,
						Description = p.Description,
						OwnerId = p.OwnerId,
						Category = p.Category,
						CreatedAt = p.CreatedAt,
						ExpiresAt = p.ExpiresAt,
						Status = p.Status,
						BudgetMin = p.BudgetMin,
						BudgetMax = p.BudgetMax,
						CurrencyCode = p.CurrencyCode,
						Tags = p.Tags,
						IsExpired = p.IsExpired
					})
					.FirstOrDefaultAsync(ct);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to get project by id {ProjectId}", projectId);
				throw;
			}
		}

		public async Task<List<ProjectDto>> GetByOwnerAsync(Guid ownerId, CancellationToken ct)
		{
			_logger.LogInformation("Getting projects by owner {OwnerId}", ownerId);

			if (ownerId == Guid.Empty)
			{
				_logger.LogWarning("OwnerId cannot be empty");
				throw new ArgumentNullException(nameof(ownerId));
			}

			try
			{
				return await _context.Set<ProjectReadModel>()
					.AsNoTracking()
					.Where(p => p.OwnerId == ownerId)
					.OrderByDescending(p => p.CreatedAt)
					.Select(p => new ProjectDto
					{
						Id = p.Id,
						Title = p.Title,
						Description = p.Description,
						OwnerId = p.OwnerId,
						Category = p.Category,
						CreatedAt = p.CreatedAt,
						ExpiresAt = p.ExpiresAt,
						Status = p.Status,
						BudgetMin = p.BudgetMin,
						BudgetMax = p.BudgetMax,
						CurrencyCode = p.CurrencyCode,
						Tags = p.Tags,
						IsExpired = p.IsExpired
					})
					.ToListAsync(ct);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to get projects by owner {OwnerId}", ownerId);
				throw;
			}
		}

		public async Task<List<ProjectDto>> GetAllAsync(CancellationToken ct)
		{
			_logger.LogInformation("Getting all projects");

			try
			{
				return await _context.Set<ProjectReadModel>()
					.AsNoTracking()
					.OrderByDescending(p => p.CreatedAt)
					.Select(p => new ProjectDto
					{
						Id = p.Id,
						Title = p.Title,
						Description = p.Description,
						OwnerId = p.OwnerId,
						Category = p.Category,
						CreatedAt = p.CreatedAt,
						ExpiresAt = p.ExpiresAt,
						Status = p.Status,
						BudgetMin = p.BudgetMin,
						BudgetMax = p.BudgetMax,
						CurrencyCode = p.CurrencyCode,
						Tags = p.Tags,
						IsExpired = p.IsExpired
					})
					.ToListAsync(ct);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to get all projects");
				throw;
			}
		}
	}
}
