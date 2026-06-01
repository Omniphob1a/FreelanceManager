using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Tasks.Application.Interfaces;
using Tasks.Domain.Interfaces;

namespace Tasks.Application.ProjectTasks.Commands.CancelProjectTask
{
	public class CancelProjectTaskCommandHandler : IRequestHandler<CancelProjectTaskCommand, Result<Unit>>
	{
		private readonly IProjectTaskRepository _projectTaskRepository;
		private readonly ILogger<CancelProjectTaskCommandHandler> _logger;
		private readonly IUnitOfWork _unitOfWork;

		public CancelProjectTaskCommandHandler(
			IProjectTaskRepository projectTaskRepository,
			ILogger<CancelProjectTaskCommandHandler> logger,
			IUnitOfWork unitOfWork)
		{
			_projectTaskRepository = projectTaskRepository ?? throw new ArgumentNullException(nameof(projectTaskRepository));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
		}

		public async Task<Result<Unit>> Handle(CancelProjectTaskCommand request, CancellationToken cancellationToken)
		{
			_logger.LogInformation("Canceling task {TaskId}", request.TaskId);

			if (request.TaskId == Guid.Empty)
				return Result.Fail<Unit>("TaskId is required.");

			var task = await _projectTaskRepository.GetByIdAsync(request.TaskId, cancellationToken);
			if (task is null)
				return Result.Fail<Unit>("Task not found.");

			try
			{
				task.Cancel(request.Reason);
			}
			catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
			{
				_logger.LogWarning(ex, "Cannot cancel task {TaskId}", request.TaskId);
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
				_logger.LogError(ex, "Error saving canceled task {TaskId}", request.TaskId);
				throw;
			}

			_logger.LogInformation("Task {TaskId} canceled", request.TaskId);
			return Result.Ok();
		}
	}
}
