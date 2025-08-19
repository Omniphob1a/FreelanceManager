using FluentResults;
using MapsterMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tasks.Application.Common.Pagination;
using Tasks.Application.DTOs;
using Tasks.Application.Interfaces;
using Tasks.Domain.Aggregate.Root;

namespace Tasks.Application.ProjectTasks.Queries.GetProjectTasksByProjectId
{
	public class GetProjectTasksByProjectIdQueryHandler : IRequestHandler<GetProjectTasksByProjectIdQuery, Result<PaginatedResult<ProjectTaskDto>>>
	{
		private readonly IProjectTaskQueryService _projectTaskQueryService;
		private readonly IMapper _mapper;
		private readonly ILogger<GetProjectTasksByProjectIdQueryHandler> _logger;

		public GetProjectTasksByProjectIdQueryHandler(
			IProjectTaskQueryService projectTaskQueryService,
			IMapper mapper,
			ILogger<GetProjectTasksByProjectIdQueryHandler> logger)
		{
			_projectTaskQueryService = projectTaskQueryService;
			_mapper = mapper;
			_logger = logger;
		}

		public async Task<Result<PaginatedResult<ProjectTaskDto>>> Handle(GetProjectTasksByProjectIdQuery request, CancellationToken cancellationToken)
		{
			_logger.LogInformation("Handling GetProjectTasksByProjectIdQuery for TaskId: {TaskId}", request.ProjectId);

			if (request.ProjectId == Guid.Empty)
			{
				_logger.LogWarning("Invalid request: empty ProjectId");
				return Result.Fail<PaginatedResult<ProjectTaskDto>>("ProjectId is required.");
			}

			try
			{
				var paginatedTasks = await _projectTaskQueryService.GetByProjectIdAsync(request.ProjectId, cancellationToken);

				var items = paginatedTasks.Items;
				var dtos = _mapper.Map<List<ProjectTaskDto>>(items);

				var paginatedResultDto = new PaginatedResult<ProjectTaskDto>(
						dtos,
						paginatedTasks.Pagination.TotalItems,
						paginatedTasks.Pagination.ActualPage,
						paginatedTasks.Pagination.ItemsPerPage);

				_logger.LogInformation("Tasks successfully retrieved, total: {Total}", paginatedResultDto.Pagination.TotalItems);
				return Result.Ok(paginatedResultDto);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error occurred while handling GetProjectTasksByProjectIdQuery for ProjectId: {ProjectId}", request.ProjectId);
				return Result.Fail<PaginatedResult<ProjectTaskDto>>("An error occurred while retrieving project tasks.");
			}
		}
	}

}
