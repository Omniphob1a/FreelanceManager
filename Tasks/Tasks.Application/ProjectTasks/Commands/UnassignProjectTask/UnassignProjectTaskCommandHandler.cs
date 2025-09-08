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

namespace Tasks.Application.ProjectTasks.Commands.UnassignProjectTask
{
	public class UnassignProjectTaskCommandHandler : IRequestHandler<UnassignProjectTaskCommand, Result<Unit>>
	{
		private readonly IProjectTaskRepository _projectTaskRepository;
		private readonly IProjectTaskQueryService _projectTaskQueryService;
		private readonly ILogger<AssignProjectTaskCommandHandler> _logger;
		private readonly IUnitOfWork _unitOfWork;
		private readonly IMemberReadRepository _membersReadRepository;

		public UnassignProjectTaskCommandHandler(
			IProjectTaskRepository projectTaskRepository,
			IProjectTaskQueryService projectTaskQueryService,
			ILogger<AssignProjectTaskCommandHandler> logger,
			IUnitOfWork unitOfWork,
			IMemberReadRepository membersReadRepository)
		{
			_projectTaskRepository = projectTaskRepository;
			_projectTaskQueryService = projectTaskQueryService;
			_logger = logger;
			_unitOfWork = unitOfWork;
			_membersReadRepository = membersReadRepository;
		}

		public async Task<Result<Unit>> Handle(UnassignProjectTaskCommand request, CancellationToken cancellationToken)
		{
			_logger.LogInformation("Unassigning task {TaskId} to assignee {AssigneeId}", request.TaskId, request.AssigneeId);

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

			if (task is null)
			{
				_logger.LogWarning("Task {TaskId} not found", request.TaskId);
				return Result.Fail<Unit>("Task not found.");
			}

			try
			{
				task.Unassign();
			}
			catch (DomainException dex)
			{
				_logger.LogWarning(dex, "Domain validation failed while unassigning task {TaskId} to {AssigneeId}", request.TaskId, request.AssigneeId);
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

			_logger.LogInformation("Task {TaskId} successfully unassigned to {AssigneeId}", request.TaskId, request.AssigneeId);
			return Result.Ok();
		}
	}
}
