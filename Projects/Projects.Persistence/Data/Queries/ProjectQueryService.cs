using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Projects.Application.Common.Filters;
using Projects.Application.Common.Pagination;
using Projects.Application.Interfaces;
using Projects.Domain.Entities;
using Projects.Persistence.Data;
using Projects.Persistence.Models;
using Projects.Persistence.Specifications;
using System.Diagnostics;

public class ProjectQueryService : IProjectQueryService
{
	private readonly ProjectsDbContext _context;
	private readonly IMapper _mapper;
	private readonly ILogger<ProjectQueryService> _logger;

	public ProjectQueryService(
		ProjectsDbContext dbContext,
		IMapper mapper,
		ILogger<ProjectQueryService> logger)
	{
		_context = dbContext;
		_mapper = mapper;
		_logger = logger;
	}

	public async Task<PaginatedResult<Project>> GetPaginatedAsync(ProjectFilter filter, CancellationToken ct)
	{
		_logger.LogDebug("Getting paginated projects with filter: {@Filter}", filter);

		try
		{
			var spec = new ProjectsByFilterSpec(filter, includePaging: true);
			var totalSpec = new ProjectsByFilterSpec(filter, includePaging: false);

			var total = await _context.Projects.WithSpecification(totalSpec).CountAsync(ct);
			var entities = await _context.Projects.WithSpecification(spec).ToListAsync(ct);

			_logger.LogDebug("Fetched {Count} projects out of {Total}", entities.Count, total);

			var mapped = _mapper.Map<List<Project>>(entities);
			return new PaginatedResult<Project>(mapped, total, filter.Page, filter.PageSize);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to get paginated projects with filter: {@Filter}", filter);
			throw;
		}
	}

	public async Task<Project?> GetByIdAsync(Guid id, CancellationToken ct)
	{
		_logger.LogDebug("Getting project by Id: {ProjectId}", id);

		try
		{
			var spec = new ProjectByIdSpec(id);
			var entity = await _context.Projects
				.WithSpecification(spec)
				.FirstOrDefaultAsync(ct);
				

			if (entity is null)
			{
				_logger.LogWarning("Project with Id {ProjectId} not found", id);
				return null;
			}

			return _mapper.Map<Project>(entity);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to get project by Id: {ProjectId}", id);
			throw;
		}
	}

	public async Task<Project?> GetByIdWithMilestonesAsync(Guid id, CancellationToken ct)
	{
		_logger.LogDebug("Getting project by Id with milestones: {ProjectId}", id);

		try
		{
			var spec = new ProjectByIdWithMilestonesSpec(id);
			var entity = await _context.Projects.WithSpecification(spec).FirstOrDefaultAsync(ct);

			if (entity is null)
			{
				_logger.LogWarning("Project with Id {ProjectId} not found (with milestones)", id);
				return null;
			}

			return _mapper.Map<Project>(entity);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to get project with milestones by Id: {ProjectId}", id);
			throw;
		}
	}

	public async Task<Project?> GetByIdWithAttachmentsAsync(Guid id, CancellationToken ct)
	{
		_logger.LogDebug("Getting project by Id with attachments: {ProjectId}", id);

		try
		{
			var spec = new ProjectByIdWithAttachmentsSpec(id);
			var entity = await _context.Projects.WithSpecification(spec).FirstOrDefaultAsync(ct);

			if (entity is null)
			{
				_logger.LogWarning("Project with Id {ProjectId} not found (with attachments)", id);
				return null;
			}

			return _mapper.Map<Project>(entity);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to get project with attachments by Id: {ProjectId}", id);
			throw;
		}
	}
}
