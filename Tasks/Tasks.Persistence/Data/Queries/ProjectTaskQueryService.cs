using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tasks.Application.Common.Filters;
using Tasks.Application.Common.Pagination;
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
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

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

		public async Task<PaginatedResult<ProjectTask>> GetByAssigneeIdAsync(Guid assigneeId, TaskFilter filter, PaginationInfo paginationInfo, CancellationToken cancellationToken)
		{
			_logger.LogInformation("Trying to get tasks by assignee ID {AssigneeId}", assigneeId);

			try
			{
				var query = _context.Tasks
					.Where(t => t.AssigneeId == assigneeId)
					.ApplyFilters(filter, paginationInfo); 

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

		public Task<PaginatedResult<ProjectTask>> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}

		public Task<PaginatedResult<ProjectTask>> GetPaginatedAsync(CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}
	}
}
