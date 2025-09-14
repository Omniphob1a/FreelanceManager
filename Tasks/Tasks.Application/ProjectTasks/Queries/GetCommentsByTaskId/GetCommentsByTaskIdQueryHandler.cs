using FluentResults;
using MapsterMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tasks.Application.DTOs;
using Tasks.Application.Interfaces;
using Tasks.Application.ProjectTasks.Commands.UpdateProjectTask;
using Tasks.Application.ProjectTasks.Queries.GetComments;
using Tasks.Domain.Interfaces;

namespace Tasks.Application.ProjectTasks.Queries.GetCommentsByTaskId
{
	public class GetCommentsByTaskIdQueryHandler : IRequestHandler<GetCommentsByTaskIdQuery, Result<List<CommentReadDto>>>
	{
		private readonly ILogger<GetCommentsByTaskIdQueryHandler> _logger;
		private readonly ICommentReadRepository _commentReadRepository;

		public GetCommentsByTaskIdQueryHandler(
			ILogger<GetCommentsByTaskIdQueryHandler> logger,
			ICommentReadRepository commentReadRepository)
		{
			_logger = logger;
			_commentReadRepository = commentReadRepository;
		}

		public async Task<Result<List<CommentReadDto>>> Handle(GetCommentsByTaskIdQuery request, CancellationToken ct)
		{
			_logger.LogInformation("Handling GetCommentsByTaskIdQuery for TaskId: {TaskId}", request.TaskId);

			try
			{
				var comments = await _commentReadRepository.GetCommentsByTaskIdAsync(request.TaskId, ct);
				return Result.Ok(comments);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error while fetching comments for tasj {TaskId}", request.TaskId);
				return Result.Fail<List<CommentReadDto>>("Unexpected error occurred.");
			}
		}
	}
}
