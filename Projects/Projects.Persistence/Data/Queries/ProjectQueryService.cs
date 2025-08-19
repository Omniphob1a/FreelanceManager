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

			var total = await _context.Projects
				.WithSpecification(totalSpec)
				.CountAsync(ct);

			var entities = await _context.Projects
				.WithSpecification(spec)
				.ToListAsync(ct);

			var items = entities != null
				? _mapper.Map<List<Project>>(entities) ?? new List<Project>()
				: new List<Project>();

			return new PaginatedResult<Project>(items, total, filter.Page, filter.PageSize);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to get paginated projects");
			return new PaginatedResult<Project>(
				new List<Project>(),
				0,
				filter.Page,
				filter.PageSize
			);
		}
	}
	
	public async Task<List<Project>> GetAllAsync()
	{
		_logger.LogDebug("Getting all projects");
		try
		{
			var entities = await _context.Projects
				.WithSpecification(new AllProjectsSpec())	
				.ToListAsync();

			if (entities is null || !entities.Any())
			{
				_logger.LogInformation("No projects found");
				return new List<Project>();
			}
			return _mapper.Map<List<Project>>(entities) ?? new List<Project>();
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to get all projects");
			return new List<Project>();
		}
	}
	public async Task<List<Project>> GetOutOfDateProjectsAsync(DateTime thresholdDate)
	{
		_logger.LogDebug("Getting out-of-date projects with threshold date: {ThresholdDate}", thresholdDate);
		try
		{
			var spec = new ProjectsOutOfDateSpec(thresholdDate);
			var entities = await _context.Projects
				.WithSpecification(spec)
				.ToListAsync();

			if (entities is null || !entities.Any())
			{
				_logger.LogInformation("No out-of-date projects found");
				return new List<Project>();
			}
			return _mapper.Map<List<Project>>(entities) ?? new List<Project>();
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to get out-of-date projects");
			return new List<Project>();
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
			_logger.LogInformation("Project status before mapping: {Status}", entity.Status);
			var project = _mapper.Map<Project>(entity);
			_logger.LogInformation("Project status after mapping: {Status}", project.Status);
			return project;
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
			var entity = await _context.Projects
				.WithSpecification(spec)
				.FirstOrDefaultAsync(ct);

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
			var entity = await _context.Projects
				.WithSpecification(spec)
				.FirstOrDefaultAsync(ct);

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

	public async Task<Project> GetFullProjectByIdAsync(Guid id, CancellationToken ct)
	{
		_logger.LogDebug("Getting full project by Id: {ProjectId}", id);

		try
		{
			var spec = new ProjectFullByIdSpec(id);
			var entity = await _context.Projects
				.WithSpecification(spec)
				.FirstOrDefaultAsync(ct);


			if (entity is null)
			{
				_logger.LogWarning("Full project with Id {ProjectId} not found", id);
				return null;
			}

			_logger.LogInformation("Project status before mapping: {Status}", entity.Status);
			var project = _mapper.Map<Project>(entity);
			_logger.LogInformation("Project status after mapping: {Status}", project.Status);
			return project;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to get full project by Id: {ProjectId}", id);
			throw;
		}
	}
}
