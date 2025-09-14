using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tasks.Application.DTOs;
using Tasks.Application.Interfaces;
using Tasks.Persistence.Models.ReadModels;

namespace Tasks.Persistence.Data.Repositories
{
	public class CommentReadRepository : ICommentReadRepository
	{
		private readonly ProjectTasksDbContext _context;
		private readonly ILogger<CommentReadRepository> _logger;
		private readonly IUserReadRepository _userReadRepository;

		public CommentReadRepository(ProjectTasksDbContext context, ILogger<CommentReadRepository> logger, IUserReadRepository userReadRepository)
		{
			_context = context;
			_logger = logger;
			_userReadRepository = userReadRepository;
		}
		public async Task<List<CommentReadDto>> GetCommentsByTaskIdAsync(Guid taskId, CancellationToken ct)
		{
			_logger.LogDebug("Getting comments for Task {TaskId}", taskId);

			try
			{
				var comments = await _context.Comments
					.AsNoTracking()
					.Where(c => c.TaskId == taskId)
					.ToListAsync(ct);

				_logger.LogDebug("Found {Count} comments for Task {TaskId}", comments.Count, taskId);

				if (!comments.Any())
				{
					_logger.LogInformation("No comments found for Task {TaskId}", taskId);
					return new List<CommentReadDto>();
				}

				var userIds = comments.Select(c => c.AuthorId).Distinct().ToList();
				var users = await _userReadRepository.GetByIdsAsync(userIds, ct);

				_logger.LogDebug("Loaded {Count} users for Task {TaskId}", users.Count, taskId);

				var dtos = comments.Select(c =>
				{
					var user = users.FirstOrDefault(u => u.Id == c.AuthorId);
					if (user == null)
					{
						_logger.LogWarning("No user found for Comment {CommentId} (AuthorId={AuthorId}) in Task {TaskId}",
							c.Id, c.AuthorId, taskId);
					}

					return new CommentReadDto
					{
						Id = c.Id,
						TaskId = c.TaskId,
						Text = c.Text,
						CreatedAt = c.CreatedAt,
						Author = user
					};
				}).ToList();

				_logger.LogDebug("Returning {Count} comment DTOs for Task {TaskId}", dtos.Count, taskId);

				return dtos;
			}
			catch (OperationCanceledException)
			{
				_logger.LogWarning("GetCommentsByTaskIdAsync was canceled for Task {TaskId}", taskId);
				throw;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to get comments for Task {TaskId}", taskId);
				return new List<CommentReadDto>();
			}
		}

	}
}
