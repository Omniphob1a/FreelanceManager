using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tasks.Application.Interfaces;
using Tasks.Application.ProjectTasks.Commands.AssignProjectTask;
using Tasks.Domain.Aggregate.Root;
using Tasks.Domain.Exceptions;
using Tasks.Domain.Interfaces;

namespace Tasks.Application.ProjectTasks.Commands.CancelProjectTask
{
	public class CancelProjectTaskCommandHandler : IRequestHandler<CancelProjectTaskCommand, Result<Guid>>
	{
		private readonly IProjectTaskRepository _projectTaskRepository;
		private readonly IProjectTaskQueryService _projectTaskQueryService;
		private readonly ILogger<AssignProjectTaskCommandHandler> _logger;
		private readonly IUnitOfWork _unitOfWork;

		public CancelProjectTaskCommandHandler(IProjectTaskRepository projectTaskRepository, IProjectTaskQueryService projectTaskQueryService, ILogger<AssignProjectTaskCommandHandler> logger, IUnitOfWork unitOfWork)
		{
			_projectTaskRepository = projectTaskRepository;
			_projectTaskQueryService = projectTaskQueryService;
			_logger = logger;
			_unitOfWork = unitOfWork;
		}

		public async Task<Result<Guid>> Handle(CancelProjectTaskCommand request, CancellationToken cancellationToken)
		{
			_logger.LogInformation("Canceling task {TaskId}", request.TaskId);

			if (request.TaskId == Guid.Empty)
			{
				_logger.LogWarning("Invalid request: empty TaskId");
				return Result.Fail<Guid>("TaskId is required.");
			}

			ProjectTask task = await _projectTaskQueryService.GetByIdAsync(request.TaskId, cancellationToken);

			if (task is null)
			{
				_logger.LogWarning("Task {TaskId} not found", request.TaskId);
				return Result.Fail<Guid>("Task not found.");
			}

			try
			{
				task.Cancel($"{request.Reason}");
			}
			catch (DomainException dex)
			{
				_logger.LogWarning(dex, "Domain validation failed while canceling task {TaskId}", request.TaskId);
				return Result.Fail<Guid>(dex.Message);
			}

			try
			{
				await _projectTaskRepository.UpdateAsync(task, cancellationToken);
				_unitOfWork.TrackEntity(task);
				await _unitOfWork.SaveChangesAsync(cancellationToken);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error saving changes for task {TaskId}", request.TaskId);
				throw;
			}

			_logger.LogInformation("Task {TaskId} successfully canceled", request.TaskId);
			return Result.Ok();
		}
	}
}
