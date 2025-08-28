using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tasks.Application.Interfaces;
using Tasks.Application.ProjectTasks.Commands.CreateProjectTask;
using Tasks.Domain.Interfaces;

namespace Tasks.Application.ProjectTasks.Commands.DeleteProjectTask
{
	public class DeleteProjectTaskCommandHandler : IRequestHandler<DeleteProjectTaskCommand, Result<Unit>>
	{
		private readonly ILogger<CreateProjectTaskCommandHandler> _logger;
		private readonly IProjectTaskRepository _projectTaskRepository;
		private readonly IProjectTaskQueryService _projectTaskQueryService;
		private readonly IUnitOfWork _unitOfWork;

		public DeleteProjectTaskCommandHandler(ILogger<CreateProjectTaskCommandHandler> logger, IProjectTaskRepository projectTaskRepository, IProjectTaskQueryService projectTaskQueryService, IUnitOfWork unitOfWork)
		{
			_logger = logger;
			_projectTaskRepository = projectTaskRepository;
			_projectTaskQueryService = projectTaskQueryService;
			_unitOfWork = unitOfWork;
		}

		public async Task<Result<Unit>> Handle(DeleteProjectTaskCommand request, CancellationToken cancellationToken)
		{
			try
			{
				var task = await _projectTaskRepository.GetByIdAsync(request.TaskId, cancellationToken);
				if (task is null)
					return Result.Fail("Task not found.");

				_logger.LogInformation("Trying to delete task with id: {Id}", request.TaskId);

				task.Delete();

				await _projectTaskRepository.DeleteAsync(request.TaskId, cancellationToken);
				_unitOfWork.TrackEntity(task);
				await _unitOfWork.SaveChangesAsync(cancellationToken);
				return Result.Ok();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to delete task with ID {Id}", request.TaskId);
				return Result.Fail("Unable to delete the task.");
			}
		}
	}
}
