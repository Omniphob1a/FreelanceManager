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

namespace Tasks.Application.ProjectTasks.Commands.AssignProjectTask
{
	public class AssignProjectTaskCommandHandler : IRequestHandler<AssignProjectTaskCommand, Result<Unit>>
	{
		private readonly IProjectTaskRepository _projectTaskRepository;
		private readonly IProjectTaskQueryService _projectTaskQueryService;
		private readonly ILogger<AssignProjectTaskCommandHandler> _logger;
		private readonly IUnitOfWork _unitOfWork;
		private readonly IMemberReadRepository _membersReadRepository;

		public AssignProjectTaskCommandHandler(
			IProjectTaskRepository projectTaskRepository,
			IProjectTaskQueryService projectTaskQueryService,
			ILogger<AssignProjectTaskCommandHandler> logger,
			IUnitOfWork unitOfWork,
			IMemberReadRepository membersReadRepository)
		{
			_projectTaskRepository = projectTaskRepository ?? throw new ArgumentNullException(nameof(projectTaskRepository));
			_projectTaskQueryService = projectTaskQueryService ?? throw new ArgumentNullException(nameof(projectTaskQueryService));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
			_membersReadRepository = membersReadRepository;
		}

		public async Task<Result<Unit>> Handle(AssignProjectTaskCommand request, CancellationToken cancellationToken)
		{
			_logger.LogInformation("Assigning task {TaskId} to assignee {AssigneeId}", request.TaskId, request.AssigneeId);

			if (request.TaskId == Guid.Empty)
			{
				_logger.LogWarning("Invalid request: empty TaskId");
				return Result.Fail<Unit>("TaskId is required.");
			}

			if (request.AssigneeId == Guid.Empty)
			{
				_logger.LogWarning("Invalid request: empty AssigneeId for Task {TaskId}", request.TaskId);
				return Result.Fail<Unit>("AssigneeId is required.");
			}

			ProjectTask task = await _projectTaskRepository.GetByIdAsync(request.TaskId, cancellationToken);

			var isExists = await _membersReadRepository.ExistsAsync(task.ProjectId, request.AssigneeId, cancellationToken);

			if (!isExists)
			{
				_logger.LogWarning("User {AssigneeId} is not a member of project {ProjectId}", request.AssigneeId, task.ProjectId);
				return Result.Fail<Unit>("Assignee must be a project member.");
			}

			if (task is null)
			{
				_logger.LogWarning("Task {TaskId} not found", request.TaskId);
				return Result.Fail<Unit>("Task not found.");
			}

			try
			{
				task.Assign(request.AssigneeId);
			}
			catch (DomainException dex)
			{
				_logger.LogWarning(dex, "Domain validation failed while assigning task {TaskId} to {AssigneeId}", request.TaskId, request.AssigneeId);
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
				_logger.LogError(ex, "Error saving changes for task {TaskId}", request.TaskId);
				throw; 
			}

			_logger.LogInformation("Task {TaskId} successfully assigned to {AssigneeId}", request.TaskId, request.AssigneeId);
			return Result.Ok();
		}
	}
}
