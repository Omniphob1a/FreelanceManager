using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tasks.Application.Common;
using Tasks.Application.Common.Filters;
using Tasks.Application.Common.Pagination;
using Tasks.Application.DTOs;
using Tasks.Application.Interfaces;
using Tasks.Domain.Aggregate.Entities;
using Tasks.Domain.Aggregate.Enums;
using Tasks.Domain.Aggregate.Enums.Tasks.Domain.Aggregate.Enums;
using Tasks.Domain.Aggregate.Root;
using Tasks.Domain.Aggregate.ValueObjects;
using Tasks.Persistence.Data;
using Tasks.Persistence.Extensions;
using Tasks.Persistence.Mappings;
using Tasks.Persistence.Models;

namespace Tasks.Persistence.Data.Repositories
{
	public class ProjectTaskQueryService : IProjectTaskQueryService
	{
		private readonly ProjectTasksDbContext _context;
		private readonly ILogger<ProjectTaskQueryService> _logger;
		private readonly ProjectTaskMapper _mapper;

		public ProjectTaskQueryService(ProjectTasksDbContext context, ILogger<ProjectTaskQueryService> logger, ProjectTaskMapper mapper)
		{
			_context = context;
			_logger = logger;
			_mapper = mapper;
		}

		public async Task<PaginatedResult<ProjectTask>> GetAllAsync(TaskFilter filter, PaginationInfo paginationInfo, CancellationToken cancellationToken)
		{
			_logger.LogInformation("Trying to get all tasks");

			try
			{
				var query = _context.Tasks
					.ApplyFilters(filter);

				var totalItems = await query.CountAsync(cancellationToken);

				var entities = await query
					.ApplyPagination(paginationInfo)
					.ToListAsync(cancellationToken);

				var items = _mapper.ToDomainCollection(entities);

				var updatedPaginationInfo = new PaginationInfo(totalItems, paginationInfo.ItemsPerPage, paginationInfo.ActualPage);

				return new PaginatedResult<ProjectTask>(items, updatedPaginationInfo);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to get tasks");
				throw;
			}
		}


		public async Task<PaginatedResult<ProjectTask>> GetByAssigneeIdAsync(Guid assigneeId, TaskFilter filter, PaginationInfo paginationInfo, CancellationToken cancellationToken)
		{
			_logger.LogInformation("Trying to get tasks by assignee ID {AssigneeId}", assigneeId);

			try
			{
				var query = _context.Tasks
					.Where(t => t.AssigneeId == assigneeId)
					.ApplyFilters(filter);
				

				var totalItems = await query.CountAsync(cancellationToken);

				var pagedEntities = await query
					.ApplyPagination(paginationInfo)
					.ToListAsync(cancellationToken);

				if (!pagedEntities.Any())
				{
					_logger.LogWarning("Tasks by assignee ID {AssigneeId} not found", assigneeId);
					return null;
				}

				var items = _mapper.ToDomainCollection(pagedEntities);
				var updatedPaginationInfo = new PaginationInfo(totalItems, paginationInfo.ItemsPerPage, paginationInfo.ActualPage);

				return new PaginatedResult<ProjectTask>(items, updatedPaginationInfo);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to get tasks by assignee ID {AssigneeId}", assigneeId);
				throw;
			}
		}

		public async Task<PaginatedResult<ProjectTask>> GetByProjectIdAsync(Guid projectId, TaskFilter filter, PaginationInfo paginationInfo, CancellationToken cancellationToken)
		{
			_logger.LogInformation("Trying to get tasks by project ID {ProjectId}", projectId);

			try
			{
				var query = _context.Tasks
					.Where(t => t.ProjectId == projectId)
					.ApplyFilters(filter)
					.ApplyPagination(paginationInfo);

				var totalItems = await query.CountAsync(cancellationToken);

				var pagedEntities = await query
					.ApplyPagination(paginationInfo)
					.ToListAsync(cancellationToken);

				if (!pagedEntities.Any())
				{
					_logger.LogWarning("Tasks by Project ID {ProjectId} not found", projectId);
					return null;
				}

				var items = _mapper.ToDomainCollection(pagedEntities);
				var updatedPaginationInfo = new PaginationInfo(totalItems, paginationInfo.ItemsPerPage, paginationInfo.ActualPage);

				return new PaginatedResult<ProjectTask>(items, updatedPaginationInfo);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to get tasks by Project ID {ProjectId}", projectId);
				throw;
			}
		}

		public async Task<ProjectTask> GetProjectTaskWithCollectionsById(Guid taskId, IEnumerable<TaskIncludeOptions> includes, CancellationToken cancellationToken)
		{
			_logger.LogInformation("Trying to get full task by ID {Id}", taskId);

			try
			{
				var query = _context.Tasks
					.Where(t => t.Id == taskId);


				foreach (var include in includes)
				{
					switch (include)
					{
						case TaskIncludeOptions.Comments:
							query = query.Include(t => t.Comments);
							break;

						case TaskIncludeOptions.TimeEntries:
							query = query.Include(t => t.TimeEntries);
							break;

					}
				}

				var entity = await query.FirstOrDefaultAsync(cancellationToken);

				if (entity == null)
				{
					_logger.LogWarning("Task with if {TaskId} not found", taskId);
					return null;
				}

				return _mapper.ToDomain(entity);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to get task by Id: {ProjectId}", taskId);
				throw;
			}
		}
	}
}
