using System;
using System.Collections.Generic;
using System.Linq;
using Tasks.Domain.Aggregate.Entities;
using Tasks.Domain.Aggregate.Enums;
using Tasks.Domain.Aggregate.Enums.Tasks.Domain.Aggregate.Enums;
using Tasks.Domain.Aggregate.Events;
using Tasks.Domain.Aggregate.ValueObjects;
using Tasks.Domain.Common;

namespace Tasks.Domain.Aggregate.Root
{
	public class ProjectTask : EntityBase
	{
		public Guid Id { get; }
		public Guid ProjectId { get; }
		public string Title { get; private set; }
		public string? Description { get; private set; }
		public Guid? AssigneeId { get; private set; }
		public Guid ReporterId { get; private set; }
		public ProjectTaskStatus Status { get; private set; }
		public TaskPriority Priority { get; private set; }
		public DateTime? DueDate { get; private set; }
		public bool IsBillable { get; private set; }
		public Money? HourlyRate { get; private set; }
		public DateTime CreatedAt { get; }
		public DateTime UpdatedAt { get; private set; }

		public TimeSpan TimeEstimated { get; private set; }

		public TimeSpan TimeSpent => _timeEntries.Aggregate(TimeSpan.Zero, (acc, e) => acc + e.Duration);

		private readonly List<TimeEntry> _timeEntries = new();
		public IReadOnlyCollection<TimeEntry> TimeEntries => _timeEntries.AsReadOnly();

		private readonly List<Comment> _comments = new();
		public IReadOnlyCollection<Comment> Comments => _comments.AsReadOnly();

		private ProjectTask(
			Guid id,
			Guid projectId,
			string title,
			string? description,
			Guid? assigneeId,
			Guid reporterId,
			DateTime createdAtUtc)
		{
			if (projectId == Guid.Empty) throw new ArgumentException("Project ID is required.", nameof(projectId));
			if (reporterId == Guid.Empty) throw new ArgumentException("Reporter ID is required.", nameof(reporterId));
			if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("Title is required.", nameof(title));

			Id = id;
			ProjectId = projectId;
			Title = title;
			Description = description;
			AssigneeId = assigneeId;
			ReporterId = reporterId;
			CreatedAt = createdAtUtc;
			UpdatedAt = createdAtUtc;
			Status = ProjectTaskStatus.ToDo;
			Priority = TaskPriority.Medium;
			TimeEstimated = TimeSpan.Zero;
		}

		public static ProjectTask CreateDraft(Guid projectId, string title, string? description, Guid reporterId, Guid? assigneeId = null)
		{
			var task = new ProjectTask(
				Guid.NewGuid(),
				projectId,
				title,
				description,
				assigneeId,
				reporterId,
				DateTime.UtcNow
			);

			task.AddDomainEvent(new TaskCreatedDomainEvent(task.Id, projectId));
			return task;
		}

		public void Delete()
		{
			AddDomainEvent(new TaskDeletedDomainEvent(Id));
		}

		public void UpdateDetails(
			string title,
			string? description,
			WorkEstimate estimate,
			DateTime? dueDate,
			bool isBillable,
			Money? hourlyRate,
			TaskPriority? priority = null)
		{
			if (string.IsNullOrWhiteSpace(title))
				throw new ArgumentException("Title is required.", nameof(title));

			if (estimate is null)
				throw new ArgumentNullException(nameof(estimate));

			Title = title;
			Description = description;
			TimeEstimated = estimate.ToTimeSpan();
			DueDate = dueDate;
			IsBillable = isBillable;
			HourlyRate = hourlyRate;
			Priority = priority ?? Priority;
			UpdatedAt = DateTime.UtcNow;

			AddDomainEvent(new TaskUpdatedDomainEvent(Id));
		}

		public void Assign(Guid assigneeId)
		{
			if (assigneeId == Guid.Empty) throw new ArgumentException("AssigneeId is required.", nameof(assigneeId));
			AssigneeId = assigneeId;
			UpdatedAt = DateTime.UtcNow;
			AddDomainEvent(new TaskAssignedDomainEvent(Id, assigneeId));
		}

		public void Unassign()
		{
			AssigneeId = null;
			UpdatedAt = DateTime.UtcNow;
			// можно добавить событие TaskUnassignedDomainEvent если нужно
		}

		public void MarkCompleted()
		{
			if (Status != ProjectTaskStatus.InProgress)
				throw new InvalidOperationException("Only tasks in progress can be completed.");

			Status = ProjectTaskStatus.Completed;
			UpdatedAt = DateTime.UtcNow;
			AddDomainEvent(new TaskCompletedDomainEvent(Id));
		}

		public void MarkInProgress()
		{
			if (Status != ProjectTaskStatus.ToDo)
				throw new InvalidOperationException("Only tasks in 'To Do' can be moved to 'In Progress'.");

			Status = ProjectTaskStatus.InProgress;
			UpdatedAt = DateTime.UtcNow;
			AddDomainEvent(new TaskStartedDomainEvent(Id));
		}

		public void Reopen()
		{
			if (Status != ProjectTaskStatus.Completed)
				throw new InvalidOperationException("Only completed tasks can be reopened.");

			Status = ProjectTaskStatus.ToDo;
			UpdatedAt = DateTime.UtcNow;
			// AddDomainEvent(new TaskReopenedDomainEvent(Id));
		}

		public void AddComment(Comment comment)
		{
			if (comment == null)
				throw new ArgumentNullException(nameof(comment));

			_comments.Add(comment);
			UpdatedAt = DateTime.UtcNow;
			AddDomainEvent(new CommentAddedDomainEvent(Id, comment.Id));
		}

		public void AddTimeEntry(TimeEntry entry)
		{
			if (entry == null)
				throw new ArgumentNullException(nameof(entry));
			if (entry.Duration < TimeSpan.Zero)
				throw new ArgumentException("Duration cannot be negative.", nameof(entry));

			_timeEntries.Add(entry);
			UpdatedAt = DateTime.UtcNow;
			AddDomainEvent(new TimeEntryAddedDomainEvent(Id, entry.Id));
		}

		public void RemoveTimeEntry(Guid timeEntryId)
		{
			var existing = _timeEntries.FirstOrDefault(t => t.Id == timeEntryId);
			if (existing == null) return;

			_timeEntries.Remove(existing);
			UpdatedAt = DateTime.UtcNow;
			// Add domain event if needed
		}

		public static ProjectTask Restore(
			Guid id,
			Guid projectId,
			string title,
			string? description,
			Guid? assigneeId,
			Guid reporterId,
			ProjectTaskStatus status,
			TaskPriority priority,
			DateTime? dueDate,
			bool isBillable,
			Money? hourlyRate,
			DateTime createdAt,
			DateTime updatedAt,
			TimeSpan timeEstimated,
			List<TimeEntry>? timeEntries,
			List<Comment>? comments)
		{
			var task = new ProjectTask(id, projectId, title, description, assigneeId, reporterId, createdAt);

			task.Status = status;
			task.Priority = priority;
			task.DueDate = dueDate;
			task.IsBillable = isBillable;
			task.HourlyRate = hourlyRate;
			task.UpdatedAt = updatedAt;
			task.TimeEstimated = timeEstimated;

			if (timeEntries != null)
				task._timeEntries.AddRange(timeEntries.Select(te => te)); 
			if (comments != null)
				task._comments.AddRange(comments.Select(c => c));

			return task;
		}

		public bool IsOverdue()
		{
			return DueDate.HasValue && DueDate.Value < DateTime.UtcNow && Status != ProjectTaskStatus.Completed;
		}
	}
}
