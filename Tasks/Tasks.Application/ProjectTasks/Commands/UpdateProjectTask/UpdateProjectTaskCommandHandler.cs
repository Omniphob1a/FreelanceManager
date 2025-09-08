using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Tasks.Application.Interfaces;
using Tasks.Domain.Aggregate.Enums;
using Tasks.Domain.Aggregate.Root;
using Tasks.Domain.Aggregate.ValueObjects;
using Tasks.Domain.Exceptions;
using Tasks.Domain.Interfaces;

namespace Tasks.Application.ProjectTasks.Commands.UpdateProjectTask
{
	public class UpdateProjectTaskCommandHandler : IRequestHandler<UpdateProjectTaskCommand, Result<Unit>>
	{
		private readonly IProjectTaskRepository _projectTaskRepository;
		private readonly IProjectTaskQueryService _projectTaskQueryService;
		private readonly ILogger<UpdateProjectTaskCommandHandler> _logger;
		private readonly IUnitOfWork _unitOfWork;
		private readonly IProjectReadRepository _projectReadRepository;

		public UpdateProjectTaskCommandHandler(
			IProjectTaskRepository projectTaskRepository,
			IProjectTaskQueryService projectTaskQueryService,
			ILogger<UpdateProjectTaskCommandHandler> logger,
			IUnitOfWork unitOfWork,
			IProjectReadRepository projectReadRepository	)
		{
			_projectTaskRepository = projectTaskRepository ?? throw new ArgumentNullException(nameof(projectTaskRepository));
			_projectTaskQueryService = projectTaskQueryService ?? throw new ArgumentNullException(nameof(projectTaskQueryService));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
			_projectReadRepository = projectReadRepository;
		}

		public async Task<Result<Unit>> Handle(UpdateProjectTaskCommand request, CancellationToken cancellationToken)
		{
			_logger.LogInformation("Updating task {TaskId}", request.TaskId);

			if (request.TaskId == Guid.Empty)
			{
				_logger.LogWarning("Invalid request: empty TaskId");
				return Result.Fail<Unit>("TaskId is required.");
			}

			ProjectTask task = await _projectTaskRepository.GetByIdAsync(request.TaskId, cancellationToken);
			if (task is null)
			{
				_logger.LogWarning("Task {TaskId} not found for update", request.TaskId);
				return Result.Fail<Unit>("Task not found.");
			}

			var isExists = await _projectReadRepository.ExistsAsync(task.ProjectId, cancellationToken);

			if (!isExists)
			{
				_logger.LogWarning("Project {ProjectId} is not exists, cannot update", task.ProjectId);
				return Result.Fail<Unit>("Project not exists");
			}

			try
			{
				task.UpdateDetails(
					task.Title,
					task.Description,
					WorkEstimate.From(request.EstimateValue, (WorkUnit)request.EstimateUnit),
					task.DueDate,
					task.IsBillable,
					Money.From(request.Amount, request.Currency)
				);
			}
			catch (ArgumentException aex)
			{
				_logger.LogWarning(aex, "Validation error while updating task {TaskId}", request.TaskId);
				return Result.Fail<Unit>(aex.Message);
			}
			catch (DomainException dex)
			{
				_logger.LogWarning(dex, "Domain error while updating task {TaskId}", request.TaskId);
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
				_logger.LogError(ex, "Unexpected error while saving task {TaskId}", request.TaskId);
				throw; 
			}

			_logger.LogInformation("Task {TaskId} updated successfully", request.TaskId);
			return Result.Ok();
		}
	}
}
