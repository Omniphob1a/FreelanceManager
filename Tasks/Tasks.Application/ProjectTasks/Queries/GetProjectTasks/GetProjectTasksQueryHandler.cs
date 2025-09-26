using FluentResults;
using Mapster;
using MapsterMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tasks.Application.Common.Pagination;
using Tasks.Application.DTOs;
using Tasks.Application.Interfaces;

namespace Tasks.Application.ProjectTasks.Queries.GetTasks
{
	public class GetProjectTasksQueryHandler : IRequestHandler<GetProjectTasksQuery, Result<PaginatedResult<TaskListItemDto>>>
	{
		private readonly IProjectTaskQueryService _projectTaskQueryService;
		private readonly IMapper _mapper;
		private readonly ILogger<GetProjectTasksQueryHandler> _logger;
		private readonly ICurrentUserService _currentUserService;

		public GetProjectTasksQueryHandler(
			IProjectTaskQueryService projectTaskQueryService,
			IMapper mapper,
			ILogger<GetProjectTasksQueryHandler> logger,
			ICurrentUserService currentUserService)
		{
			_projectTaskQueryService = projectTaskQueryService;
			_mapper = mapper;
			_logger = logger;
			_currentUserService = currentUserService;
		}

		public async Task<Result<PaginatedResult<TaskListItemDto>>> Handle(
			GetProjectTasksQuery request,
			CancellationToken cancellationToken)
		{
			_logger.LogInformation("Handling GetProjectTasksQuery");

			if (request.filter == null)
			{
				_logger.LogWarning("Filter is null");
				return Result.Fail<PaginatedResult<TaskListItemDto>>("Filter cannot be null");
			}
			if (request.paginationInfo == null)
			{
				_logger.LogWarning("PaginationInfo is null");
				return Result.Fail<PaginatedResult<TaskListItemDto>>("PaginationInfo cannot be null");
			}

			try
			{
				if (request.filter.OnlyMyTasks)
				{
					request.filter.CurrentUserId = _currentUserService.UserId;
				}

				var paginatedResult = await _projectTaskQueryService.GetAllAsync(
					request.filter,
					request.paginationInfo,
					cancellationToken
				);

				var dtos = _mapper.Map<List<TaskListItemDto>>(paginatedResult.Items);
				_logger.LogInformation("Handler → после маппинга {Count} DTO", dtos.Count);

				var updatedPaginationInfo = new PaginationInfo(
					paginatedResult.Pagination.TotalItems,
					request.paginationInfo.ItemsPerPage,
					request.paginationInfo.ActualPage
				);

				var paginatedResultDto = new PaginatedResult<TaskListItemDto>(dtos, updatedPaginationInfo);
				return paginatedResultDto;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error retrieving tasks with filter {@Filter}", request.filter);
				return Result.Fail<PaginatedResult<TaskListItemDto>>("Unexpected error occurred");
			}
		}
	}
}
