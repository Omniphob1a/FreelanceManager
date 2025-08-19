using FluentResults;
using MapsterMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tasks.Application.DTOs;
using Tasks.Application.Interfaces;
using Tasks.Application.ProjectTasks.Commands.UpdateProjectTask;
using Tasks.Domain.Aggregate.Root;

namespace Tasks.Application.ProjectTasks.Queries.GetProjectTaskById
{
	public class GetProjectTaskByIdQueryHandler : IRequestHandler<GetProjectTaskByIdQuery, Result<ProjectTaskDto>>
	{
		private readonly IProjectTaskQueryService _projectTaskQueryService;
		private readonly ILogger<UpdateProjectTaskCommandHandler> _logger;
		private readonly IMapper _mapper;

		public GetProjectTaskByIdQueryHandler(
			IProjectTaskQueryService projectTaskQueryService,
			ILogger<UpdateProjectTaskCommandHandler> logger,
			IMapper mapper)
		{
			_projectTaskQueryService = projectTaskQueryService;
			_logger = logger;
			_mapper = mapper;
		}

		public async Task<Result<ProjectTaskDto>> Handle(GetProjectTaskByIdQuery request, CancellationToken cancellationToken)
		{
			_logger.LogInformation("Handling GetProjectTaskByIdQuery for TaskId: {TaskId}", request.TaskId);

			if (request.TaskId == Guid.Empty)
			{
				_logger.LogWarning("Invalid request: empty TaskId");
				return Result.Fail("TaskId is required.");
			}

			ProjectTask task = await _projectTaskQueryService.GetByIdAsync(request.TaskId, cancellationToken);

			try
			{
				var dto = _mapper.Map<ProjectTaskDto>(task);
				_logger.LogInformation("Successfully retrieved task {TaskId}", request.TaskId);
				return Result.Ok(dto);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Unexpected error while retrieving task {TaskId}", request.TaskId);
				return Result.Fail("An unexpected error occurred while retrieving the task.");
			}	
		}
	}
}
