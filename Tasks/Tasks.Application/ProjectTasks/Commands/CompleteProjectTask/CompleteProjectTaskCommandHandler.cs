using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Tasks.Application.Interfaces;
using Tasks.Domain.Aggregate.Root;
using Tasks.Domain.Exceptions;
using Tasks.Domain.Interfaces;

namespace Tasks.Application.ProjectTasks.Commands.CompleteProjectTask
{
	public class CompleteProjectTaskCommandHandler : IRequestHandler<CompleteProjectTaskCommand, Result<Unit>>
	{
		private readonly IProjectTaskRepository _projectTaskRepository;
		private readonly IProjectTaskQueryService _projectTaskQueryService;
		private readonly ILogger<CompleteProjectTaskCommandHandler> _logger;
		private readonly IUnitOfWork _unitOfWork;

		public CompleteProjectTaskCommandHandler(
			IProjectTaskRepository projectTaskRepository,
			IProjectTaskQueryService projectTaskQueryService,
			ILogger<CompleteProjectTaskCommandHandler> logger,
			IUnitOfWork unitOfWork)
		{
			_projectTaskRepository = projectTaskRepository;
			_projectTaskQueryService = projectTaskQueryService;
			_logger = logger;
			_unitOfWork = unitOfWork;
		}

		public async Task<Result<Unit>> Handle(CompleteProjectTaskCommand request, CancellationToken cancellationToken)
		{
			_logger.LogInformation("Completing task {TaskId}", request.TaskId);

			if (request.TaskId == Guid.Empty)
			{
				_logger.LogWarning("Invalid request: empty TaskId");
				return Result.Fail<Unit>("TaskId is required.");
			}

			ProjectTask task = await _projectTaskRepository.GetByIdAsync(request.TaskId, cancellationToken);
			if (task is null)
			{
				_logger.LogWarning("Task {TaskId} not found for completion", request.TaskId);
				return Result.Fail<Unit>("Task not found.");
			}

			try
			{
				task.MarkCompleted();
			}
			catch (DomainException dex)
			{
				_logger.LogWarning(dex, "Domain error while completing task {TaskId}", request.TaskId);
				return Result.Fail<Unit>(dex.Message);
			}

			try
			{
				await _projectTaskRepository.UpdateAsync(task, cancellationToken);
				_unitOfWork.TrackEntity(task);
				await _unitOfWork.SaveChangesAsync(cancellationToken);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Unexpected error while saving task {TaskId} after completion", request.TaskId);
				throw;
			}

			_logger.LogInformation("Task {TaskId} completed successfully", request.TaskId);
			return Result.Ok();
		}
	}
}
