using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tasks.Application.Interfaces;
using Tasks.Application.ProjectTasks.Commands.AddTimeEntry;
using Tasks.Domain.Aggregate.Entities;
using Tasks.Domain.Exceptions;
using Tasks.Domain.Interfaces;

namespace Tasks.Application.ProjectTasks.Commands.AddComment
{
	public class AddCommentCommandHandler : IRequestHandler<AddCommentCommand, Result<Unit>>
	{
		private readonly IProjectTaskRepository _projectTaskRepository;
		private readonly IProjectTaskQueryService _projectTaskQueryService;
		private readonly IUnitOfWork _unitOfWork;
		private readonly ILogger<AddCommentCommandHandler> _logger;

		public AddCommentCommandHandler(
			IProjectTaskRepository projectTaskRepository,
			IProjectTaskQueryService projectTaskQueryService,
			IUnitOfWork unitOfWork,
			ILogger<AddCommentCommandHandler> logger)
		{
			_projectTaskRepository = projectTaskRepository;
			_projectTaskQueryService = projectTaskQueryService;
			_unitOfWork = unitOfWork;
			_logger = logger;
		}

		public async Task<Result<Unit>> Handle(AddCommentCommand request, CancellationToken cancellationToken)
		{
			_logger.LogInformation(
				"Adding comment for TaskId: {TaskId}, AuthorId: {AuthorId}",
				request.TaskId, request.AuthorId);

			var task = await _projectTaskRepository.GetByIdForUpdateAsync(request.TaskId, cancellationToken);
			if (task is null)
			{
				_logger.LogWarning("Task {TaskId} not found when adding comment", request.TaskId);
				return Result.Fail<Unit>("Task not found.");
			}

			try
			{
				var comment = Comment.Create(
					authorId: request.AuthorId,
					text: request.Text,
					taskId: request.TaskId,
					createdAtUtc: DateTime.UtcNow);

				task.AddComment(comment);

				await _projectTaskRepository.UpdateAsync(task, cancellationToken);
				_unitOfWork.TrackEntity(task);
				await _unitOfWork.SaveChangesAsync(cancellationToken);
			}
			catch (ArgumentException ex)
			{
				_logger.LogWarning(ex, "Validation error while adding comment to Task {TaskId}", request.TaskId);
				return Result.Fail<Unit>(ex.Message);
			}
			catch (DomainException ex)
			{
				_logger.LogWarning(ex, "Domain error while adding comment to Task {TaskId}", request.TaskId);
				return Result.Fail<Unit>(ex.Message);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Unexpected error while adding comment to Task {TaskId}", request.TaskId);
				return Result.Fail<Unit>("Unexpected error occurred while adding comment.");
			}

			_logger.LogInformation("Comment successfully added to Task {TaskId}", request.TaskId);
			return Result.Ok();
		}
	}
}
