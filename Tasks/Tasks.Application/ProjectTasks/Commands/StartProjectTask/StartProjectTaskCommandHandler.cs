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

namespace Tasks.Application.ProjectTasks.Commands.StartProjectTask
{
	public class StartProjectTaskCommandHandler : IRequestHandler<StartProjectTaskCommand, Result<Unit>>
	{
		private readonly IProjectTaskRepository _projectTaskRepository;
		private readonly IProjectTaskQueryService _projectTaskQueryService;
		private readonly ILogger<StartProjectTaskCommandHandler> _logger;
		private readonly IUnitOfWork _unitOfWork;

		public StartProjectTaskCommandHandler(
			IProjectTaskRepository projectTaskRepository,
			IProjectTaskQueryService projectTaskQueryService,
			ILogger<StartProjectTaskCommandHandler> logger,
			IUnitOfWork unitOfWork)
		{
			_projectTaskRepository = projectTaskRepository ?? throw new ArgumentNullException(nameof(projectTaskRepository));
			_projectTaskQueryService = projectTaskQueryService ?? throw new ArgumentNullException(nameof(projectTaskQueryService));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
		}

		public async Task<Result<Unit>> Handle(StartProjectTaskCommand request, CancellationToken cancellationToken)
		{
			_logger.LogInformation("Starting task {TaskId}", request.TaskId);

			if (request.TaskId == Guid.Empty)
			{
				_logger.LogWarning("Invalid request: empty TaskId");
				return Result.Fail<Unit>("TaskId is required.");
			}

			ProjectTask task = await _projectTaskQueryService.GetByIdAsync(request.TaskId, cancellationToken);
			if (task is null)
			{
				_logger.LogWarning("Task {TaskId} not found for start", request.TaskId);
				return Result.Fail<Unit>("Task not found.");
			}

			try
			{
				task.Start();
			}
			catch (DomainException dex)
			{
				_logger.LogWarning(dex, "Domain error while starting task {TaskId}", request.TaskId);
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
				_logger.LogError(ex, "Unexpected error while saving task {TaskId} after start", request.TaskId);
				throw; 
			}

			_logger.LogInformation("Task {TaskId} started successfully", request.TaskId);
			return Result.Ok();
		}
	}
}
