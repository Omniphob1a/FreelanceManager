using Microsoft.Extensions.Logging;
using Tasks.Domain.Aggregate.Entities;
using Tasks.Domain.Aggregate.Enums;
using Tasks.Domain.Aggregate.Enums.Tasks.Domain.Aggregate.Enums;
using Tasks.Domain.Aggregate.Root;
using Tasks.Domain.Aggregate.ValueObjects;
using Tasks.Persistence.Models;

namespace Tasks.Persistence.Mappings
{
	public class ProjectTaskMapper
	{
		private readonly ILogger<ProjectTaskMapper> _logger;
		private readonly TimeEntryMapper _timeEntryMapper;
		private readonly CommentMapper _commentMapper;

		public ProjectTaskMapper(
			ILogger<ProjectTaskMapper> logger,
			TimeEntryMapper timeEntryMapper,
			CommentMapper commentMapper)
		{
			_logger = logger;
			_timeEntryMapper = timeEntryMapper;
			_commentMapper = commentMapper;
		}

		public ProjectTaskEntity ToEntity(ProjectTask task)
		{
			var entity = new ProjectTaskEntity
			{
				Id = task.Id,
				ProjectId = task.ProjectId,
				Title = task.Title,
				Description = task.Description,
				AssigneeId = task.AssigneeId,
				ReporterId = task.ReporterId,
				Status = (int)task.Status,
				Priority = (int)task.Priority,
				DueDate = task.DueDate,
				IsBillable = task.IsBillable,
				CreatedAt = task.CreatedAt,
				UpdatedAt = task.UpdatedAt,
				TimeEstimatedTicks = task.TimeEstimated.Ticks,
				TimeEntries = task.TimeEntries
					.Select(te => _timeEntryMapper.ToEntity(te, task.Id))
					.ToList(),
				Comments = task.Comments
					.Select(c => _commentMapper.ToEntity(c, task.Id))
					.ToList()
			};

			return entity;
		}

		public List<ProjectTask> ToDomainCollection(List<ProjectTaskEntity> entities)
		{
			var tasks = entities
				.Select(entities => ToDomain(entities))
				.ToList();

			return tasks;
		}

		public void UpdateEntity(ProjectTask task, ProjectTaskEntity entity)
		{
			entity.Title = task.Title;
			entity.Description = task.Description;
			entity.AssigneeId = task.AssigneeId;
			entity.ReporterId = task.ReporterId;
			entity.Status = (int)task.Status;
			entity.Priority = (int)task.Priority;
			entity.DueDate = task.DueDate;
			entity.IsBillable = task.IsBillable;
			entity.UpdatedAt = task.UpdatedAt;
			entity.TimeEstimatedTicks = task.TimeEstimated.Ticks;
		}

		public ProjectTask ToDomain(ProjectTaskEntity entity)
		{
			var timeEntries = entity.TimeEntries?
				.Select(te => _timeEntryMapper.ToDomain(te))
				.ToList() ?? new();

			var comments = entity.Comments?
				.Select(c => _commentMapper.ToDomain(c))
				.ToList() ?? new();

			var task = ProjectTask.Restore(
				id: entity.Id,
				projectId: entity.ProjectId,
				title: entity.Title,
				description: entity.Description,
				assigneeId: entity.AssigneeId,
				reporterId: entity.ReporterId,
				status: (ProjectTaskStatus)entity.Status,
				priority: (TaskPriority)entity.Priority,
				dueDate: entity.DueDate,
				isBillable: entity.IsBillable,
				hourlyRate: null, 
				createdAt: entity.CreatedAt,
				updatedAt: entity.UpdatedAt,
				timeEstimated: TimeSpan.FromTicks(entity.TimeEstimatedTicks),
				timeEntries: timeEntries,
				comments: comments
			);

			return task;
		}
		public TimeEntryEntity MapTimeEntryToEntity(TimeEntry timeEntry)
		{
			return _timeEntryMapper.ToEntity(timeEntry, timeEntry.TaskId);
		}

		public CommentEntity MapCommentToEntity(Comment comment)
		{
			return _commentMapper.ToEntity(comment, comment.TaskId);
		}
	}
}
