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

namespace Tasks.Application.ProjectTasks.Commands.CreateProjectTask
{
	public class CreateProjectTaskCommandHandler : IRequestHandler<CreateProjectTaskCommand, Result<Guid>>
	{
		private readonly ILogger<CreateProjectTaskCommandHandler> _logger;
		private readonly IProjectTaskRepository _projectTaskRepository;
		private readonly IProjectTaskQueryService _projectTaskQueryService;
		private readonly IProjectService _projectService;
		private readonly IUnitOfWork _unitOfWork;

		public CreateProjectTaskCommandHandler(
			ILogger<CreateProjectTaskCommandHandler> logger,
			IProjectTaskRepository projectTaskRepository,
			IProjectTaskQueryService projectTaskQueryService,
			IProjectService projectService,
			IUnitOfWork unitOfWork)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_projectTaskRepository = projectTaskRepository ?? throw new ArgumentNullException(nameof(projectTaskRepository));
			_projectTaskQueryService = projectTaskQueryService ?? throw new ArgumentNullException(nameof(projectTaskQueryService));
			_projectService = projectService ?? throw new ArgumentNullException(nameof(projectService));
			_unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
		}

		public async Task<Result<Guid>> Handle(CreateProjectTaskCommand request, CancellationToken cancellationToken)
		{
			_logger.LogInformation("Creating new task. ProjectId: {ProjectId}, Title: {Title}, AssigneeId: {AssigneeId}",
				request.ProjectId, request.Title, request.AssigneeId);

			if (request.ProjectId == Guid.Empty)
			{
				_logger.LogWarning("Invalid request: empty ProjectId");
				return Result.Fail<Guid>("ProjectId is required.");
			}

			if (string.IsNullOrWhiteSpace(request.Title))
			{
				_logger.LogWarning("Invalid request: empty Title for Project {ProjectId}", request.ProjectId);
				return Result.Fail<Guid>("Title is required.");
			}

			bool projectExists = await _projectService.ExistsAsync(request.ProjectId, cancellationToken);
			if (!projectExists)
			{
				_logger.LogWarning("Project {ProjectId} not found", request.ProjectId);
				return Result.Fail<Guid>("Project not found.");
			}

			ProjectTask task;
			try
			{
				task = ProjectTask.CreateDraft(
					request.ProjectId,
					request.Title,
					request.Description,
					request.ReporterId
				);
			}
			catch (DomainException ex)
			{
				_logger.LogWarning(ex, "Domain error while creating task in Project {ProjectId}. Title: {Title}", request.ProjectId, request.Title);
				return Result.Fail<Guid>(ex.Message);
			}

			try
			{
				await _projectTaskRepository.AddAsync(task, cancellationToken);
				_unitOfWork.TrackEntity(task); 
				await _unitOfWork.SaveChangesAsync(cancellationToken);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Unexpected error while saving task in Project {ProjectId}. Title: {Title}", request.ProjectId, request.Title);
				throw;
			}

			_logger.LogInformation("Task created successfully with ID: {TaskId} in Project {ProjectId}", task.Id, request.ProjectId);
			return Result.Ok(task.Id);
		}
	}
}
