using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tasks.Domain.Aggregate.Entities;
using Tasks.Domain.Aggregate.Enums;
using Tasks.Domain.Aggregate.Root;
using Tasks.Domain.Aggregate.ValueObjects;
using Tasks.Domain.Interfaces;
using Tasks.Persistence.Data;
using Tasks.Persistence.Mappings;
using Tasks.Persistence.Models;

namespace Tasks.Persistence.Data.Repositories
{
	public class ProjectTaskRepository : IProjectTaskRepository
	{
		private readonly ProjectTasksDbContext _context;
		private readonly ILogger<ProjectTaskRepository> _logger;
		private readonly ProjectTaskMapper _mapper;

		public ProjectTaskRepository(
			ProjectTasksDbContext context,
			ILogger<ProjectTaskRepository> logger,
			ProjectTaskMapper mapper)
		{
			_context = context;
			_logger = logger;
			_mapper = mapper;
		}

		public async Task AddAsync(ProjectTask task, CancellationToken cancellationToken)
		{
			if (task == null)
			{
				_logger.LogWarning("Trying to add null task");
				throw new ArgumentNullException(nameof(task));
			}

			try
			{
				_logger.LogDebug("Adding task with ID {TaskId}", task.Id);

				var entity = _mapper.ToEntity(task);

				if (entity.TimeEntries != null)
				{
					foreach (var te in entity.TimeEntries)
						te.TaskId = entity.Id;
				}

				if (entity.Comments != null)
				{
					foreach (var c in entity.Comments)
						c.TaskId = entity.Id;
				}

				await _context.Tasks.AddAsync(entity, cancellationToken);

				_logger.LogInformation("Task with ID {TaskId} added successfully (entity prepared)", task.Id);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to add task with ID {TaskId}", task?.Id);
				throw;
			}
		}

		public async Task UpdateAsync(ProjectTask task, CancellationToken cancellationToken)
		{
			if (task == null)
			{
				_logger.LogWarning("Trying to update null task");
				throw new ArgumentNullException(nameof(task));
			}

			try
			{
				_logger.LogDebug("Updating task with ID {TaskId}", task.Id);

				var existing = await _context.Tasks
					.Include(t => t.TimeEntries)
					.Include(t => t.Comments)
					.FirstOrDefaultAsync(t => t.Id == task.Id, cancellationToken);

				if (existing == null)
				{
					_logger.LogWarning("Task with ID {TaskId} not found for update", task.Id);
					throw new KeyNotFoundException($"Task with ID {task.Id} not found.");
				}

				_mapper.UpdateEntity(task, existing);

				if (task.TimeEntries?.Any() == true)
				{
					await _context
						.Entry(existing)
						.Collection(e => e.TimeEntries)
						.LoadAsync(cancellationToken);

					await SyncTimeEntriesAsync(existing, task.TimeEntries, cancellationToken);
				}

				if (task.Comments?.Any() == true)
				{
					await _context
						.Entry(existing)
						.Collection(e => e.Comments)
						.LoadAsync(cancellationToken);

					await SyncCommentsAsync(existing, task.Comments, cancellationToken);
				}

				_logger.LogInformation("Task with ID {TaskId} updated successfully (entity patched)", task.Id);

				var entries = _context.ChangeTracker.Entries()
					.Where(e => e.State != EntityState.Unchanged)
					.ToList();

				if (!entries.Any())
				{
					_logger.LogWarning("No tracked changes detected before SaveChanges for task {TaskId}", task.Id);
				}
				else
				{
					foreach (var entry in entries)
					{
						var entityType = entry.Entity.GetType().Name;
						var state = entry.State;
						var idProperty = entry.Properties.FirstOrDefault(p => p.Metadata.IsPrimaryKey());
						var id = idProperty?.CurrentValue?.ToString() ?? "N/A";

						_logger.LogInformation("Tracked entity: {EntityType}, ID: {Id}, State: {State}",
							entityType, id, state);
					}
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to update task with ID {TaskId}", task.Id);
				throw;
			}
		}

		public async Task DeleteAsync(Guid taskId, CancellationToken cancellationToken)
		{
			if (taskId == Guid.Empty)
			{
				_logger.LogWarning("DeleteAsync called with empty TaskId");
				throw new ArgumentException("Task ID cannot be empty.", nameof(taskId));
			}

			try
			{
				_logger.LogDebug("Deleting task with ID {TaskId}", taskId);

				var existing = await _context.Tasks
					.Include(t => t.TimeEntries)
					.Include(t => t.Comments)
					.FirstOrDefaultAsync(t => t.Id == taskId, cancellationToken);

				if (existing == null)
				{
					_logger.LogWarning("Task with ID {TaskId} not found for deletion", taskId);
					throw new KeyNotFoundException($"Task with ID {taskId} not found.");
				}

				_context.Tasks.Remove(existing);

				_logger.LogInformation("Task with ID {TaskId} deleted successfully (entity removed)", taskId);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to delete task with ID: {TaskId}", taskId);
				throw;
			}
		}

		private async Task SyncTimeEntriesAsync(ProjectTaskEntity taskEntity, IEnumerable<TimeEntry> domainEntries, CancellationToken cancellationToken)
		{
			var list = domainEntries?.ToList() ?? new List<TimeEntry>();
			var existingDict = taskEntity.TimeEntries.ToDictionary(e => e.Id);
			var newDict = list.ToDictionary(e => e.Id);

			var toRemove = taskEntity.TimeEntries.Where(e => !newDict.ContainsKey(e.Id)).ToList();
			foreach (var rem in toRemove)
			{
				taskEntity.TimeEntries.Remove(rem);
			}

			foreach (var te in list)
			{
				if (existingDict.TryGetValue(te.Id, out var existingEntity))
				{
					existingEntity.UserId = te.UserId;
					existingEntity.StartedAt = te.Period.Start;
					existingEntity.EndedAt = te.Period.End;
					existingEntity.Description = te.Description;
					existingEntity.IsBillable = te.IsBillable;
					existingEntity.CreatedAt = te.CreatedAt;

					if (te.HourlyRateSnapshot != null)
					{
						existingEntity.HourlyRateAmount = te.HourlyRateSnapshot.Amount;
						existingEntity.HourlyRateCurrency = te.HourlyRateSnapshot.Currency;
					}
					else
					{
						existingEntity.HourlyRateAmount = null;
						existingEntity.HourlyRateCurrency = null;
					}
				}
				else
				{
					var newEntity = _mapper.MapTimeEntryToEntity(te);
					newEntity.TaskId = taskEntity.Id;
					_context.Entry(newEntity).State = EntityState.Added;
					taskEntity.TimeEntries.Add(newEntity);
				}
			}
		}

		private async Task SyncCommentsAsync(ProjectTaskEntity taskEntity, IEnumerable<Comment> domainComments, CancellationToken cancellationToken)
		{
			var list = domainComments?.ToList() ?? new List<Comment>();
			var existingDict = taskEntity.Comments.ToDictionary(e => e.Id);
			var newDict = list.ToDictionary(c => c.Id);

			var toRemove = taskEntity.Comments.Where(e => !newDict.ContainsKey(e.Id)).ToList();
			foreach (var rem in toRemove)
			{
				taskEntity.Comments.Remove(rem);
			}

			foreach (var c in list)
			{
				if (existingDict.TryGetValue(c.Id, out var existingEntity))
				{
					existingEntity.AuthorId = c.AuthorId;
					existingEntity.Text = c.Text;
					existingEntity.CreatedAt = c.CreatedAt;
				}
				else
				{
					var newEntity = _mapper.MapCommentToEntity(c);
					newEntity.TaskId = taskEntity.Id;
					_context.Entry(newEntity).State = EntityState.Added;
					taskEntity.Comments.Add(newEntity);
				}
			}
		}

		public async Task<ProjectTask> GetByIdAsync(Guid TaskId, CancellationToken cancellationToken)
		{
			_logger.LogInformation("Trying to get task by ID {Id}", TaskId);

			try
			{
				var entity = await _context.Tasks
					.FirstOrDefaultAsync(t => t.Id == TaskId, cancellationToken);

				if (entity == null)
				{
					_logger.LogWarning("Task with if {TaskId} not found", TaskId);
					return null;
				}

				return _mapper.ToDomain(entity);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to get task by Id: {ProjectId}", TaskId);
				throw;
			}

		}

		public async Task<ProjectTask> GetFullTaskByIdAsync(Guid TaskId, CancellationToken cancellationToken)
		{
			_logger.LogInformation("Trying to get full task by ID {Id}", TaskId);

			try
			{
				var entity = await _context.Tasks
					.Include(t => t.TimeEntries)
					.Include(t => t.Comments)
					.FirstOrDefaultAsync(t => t.Id == TaskId, cancellationToken);

				if (entity == null)
				{
					_logger.LogWarning("Task with if {TaskId} not found", TaskId);
					return null;
				}

				return _mapper.ToDomain(entity);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to get task by Id: {ProjectId}", TaskId);
				throw;
			}
		}
	}
}
