using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Tasks.Application.Interfaces;
using Tasks.Domain.Aggregate.Enums;
using Tasks.Domain.Aggregate.Enums.Tasks.Domain.Aggregate.Enums;
using Tasks.Domain.Aggregate.Root;
using Tasks.Domain.Aggregate.ValueObjects;
using Tasks.Domain.Exceptions;
using Tasks.Domain.Interfaces;

namespace Tasks.Application.ProjectTasks.Commands.UpdateProjectTask
{
	public class UpdateProjectTaskCommandHandler : IRequestHandler<UpdateProjectTaskCommand, Result<Unit>>
	{
		private readonly ILogger<UpdateProjectTaskCommandHandler> _logger;
		private readonly IProjectTaskRepository _projectTaskRepository;
		private readonly IUnitOfWork _unitOfWork;
		private readonly IProjectReadRepository _projectReadRepository;

		public UpdateProjectTaskCommandHandler(
			ILogger<UpdateProjectTaskCommandHandler> logger,
			IProjectTaskRepository projectTaskRepository,
			IUnitOfWork unitOfWork,
			IProjectReadRepository projectReadRepository)
		{
			_logger = logger;
			_projectTaskRepository = projectTaskRepository;
			_unitOfWork = unitOfWork;
			_projectReadRepository = projectReadRepository;
		}

		public async Task<Result<Unit>> Handle(UpdateProjectTaskCommand request, CancellationToken cancellationToken)
		{
			_logger.LogInformation("Updating task {TaskId}. Title: {Title}, AssigneeId: {AssigneeId}",
				request.TaskId, request.Title, request.AssigneeId);

			if (request.TaskId == Guid.Empty)
			{
				_logger.LogWarning("Invalid request: empty TaskId");
				return Result.Fail<Unit>("TaskId is required.");
			}

			ProjectTask? task = await _projectTaskRepository.GetByIdAsync(request.TaskId, cancellationToken);
			if (task is null)
			{
				_logger.LogWarning("Task {TaskId} not found", request.TaskId);
				return Result.Fail<Unit>("Task not found.");
			}

			bool projectExists = await _projectReadRepository.ExistsAsync(task.ProjectId, cancellationToken);
			if (!projectExists)
			{
				_logger.LogWarning("Project {ProjectId} not found for Task {TaskId}", task.ProjectId, request.TaskId);
				return Result.Fail<Unit>("Project not found.");
			}

			try
			{
				task.UpdateDetails(
					request.ProjectId,
					request.Title,
					request.Description,
					request.TimeEstimated,
					request.DueDate,
					request.IsBillable,
					(TaskPriority)request.Priority,
					request.AssigneeId
				);
			}
			catch (DomainException ex)
			{
				_logger.LogWarning(ex, "Domain error while updating Task {TaskId}", request.TaskId);
				return Result.Fail<Unit>(ex.Message);
			}
			catch (ArgumentException ex)
			{
				_logger.LogWarning(ex, "Validation error while updating Task {TaskId}", request.TaskId);
				return Result.Fail<Unit>(ex.Message);
			}

			try
			{
				await _projectTaskRepository.UpdateAsync(task, cancellationToken);
				_unitOfWork.TrackEntity(task);
				await _unitOfWork.SaveChangesAsync(cancellationToken);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Unexpected error while saving Task {TaskId}", request.TaskId);
				throw;
			}

			_logger.LogInformation("Task {TaskId} updated successfully", request.TaskId);
			return Result.Ok();
		}
	}
}
