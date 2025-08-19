using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MapsterMapper;
using Projects.Domain.Entities;
using Projects.Domain.Repositories;
using Projects.Domain.Enums;
using Projects.Persistence.Data;
using Projects.Persistence.Models;
using Projects.Shared.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Projects.Persistence.Repositories
{
	public class ProjectRepository : IProjectRepository
	{
		private readonly ProjectsDbContext _context;
		private readonly IMapper _mapper;
		private readonly ILogger<ProjectRepository> _logger;

		public ProjectRepository(
			ProjectsDbContext context,
			IMapper mapper,
			ILogger<ProjectRepository> logger)
		{
			_context = context;
			_mapper = mapper;
			_logger = logger;
		}

		public async Task<bool> ExistsAsync(Guid projectId, CancellationToken cancellationToken = default)
		{
			if (projectId == Guid.Empty)
			{
				_logger.LogWarning("ExistsAsync called with empty ProjectId");
				throw new ArgumentException("Project ID cannot be empty.", nameof(projectId));
			}

			try
			{
				_logger.LogDebug("Checking if project with ID {ProjectId} exists", projectId);

				var exists = await _context.Projects
					.AsNoTracking()
					.AnyAsync(p => p.Id == projectId, cancellationToken);

				_logger.LogDebug("Project with ID {ProjectId} exists: {Exists}", projectId, exists);
				return exists;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to check existence of project with ID: {ProjectId}", projectId);
				throw;
			}
		}

		public async Task AddAsync(Project project, CancellationToken cancellationToken = default)
		{
			if (project == null)
			{
				_logger.LogWarning("Trying to add null project");
				throw new ArgumentNullException(nameof(project));
			}

			try
			{
				_logger.LogDebug("Adding project with ID {ProjectId}", project.Id);

				var projectEntity = _mapper.Map<ProjectEntity>(project);
				_logger.LogInformation("Project createdAt after mapping: {CreatedAt}", projectEntity.CreatedAt);
				await _context.Projects.AddAsync(projectEntity, cancellationToken);

				_logger.LogInformation("Project with ID {ProjectId} added successfully", project.Id);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to add project with ID {ProjectId}", project.Id);
				throw;
			}
		}

		public async Task UpdateAsync(Project project, CancellationToken cancellationToken = default)
		{
			if (project == null)
			{
				_logger.LogWarning("Trying to update null project");
				throw new ArgumentNullException(nameof(project));
			}

			try
			{
				_logger.LogDebug("Updating project with ID {ProjectId}", project.Id);

				var existingEntity = await _context.Projects
					.Include(p => p.Attachments)
					.Include(p => p.Milestones)
					.FirstOrDefaultAsync(p => p.Id == project.Id, cancellationToken);

				if (existingEntity == null)
				{
					_logger.LogWarning("Project with ID {ProjectId} not found for update", project.Id);
					throw new KeyNotFoundException($"Project with ID {project.Id} not found.");
				}

				_mapper.Map(project, existingEntity);

				if (project.Milestones?.Any() == true)
				{
					await _context.Entry(existingEntity)
						.Collection(p => p.Milestones)
						.LoadAsync(cancellationToken);

					await SyncMilestonesAsync(existingEntity, project.Milestones, cancellationToken);
				}

				if (project.Attachments?.Any() == true)
				{
					await _context.Entry(existingEntity)
						.Collection(p => p.Attachments)
						.LoadAsync(cancellationToken);

					await SyncAttachmentsAsync(existingEntity, project.Attachments, cancellationToken);
				}

				_logger.LogInformation("Project with ID {ProjectId} updated successfully", project.Id);
				var entries = _context.ChangeTracker.Entries()
					.Where(e => e.State != EntityState.Unchanged)
					.ToList();

				if (!entries.Any())
				{
					_logger.LogWarning("No tracked changes detected before SaveChanges");
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
				_logger.LogError(ex, "Failed to update project with ID {ProjectId}", project.Id);
				throw;
			}
		}

		public async Task DeleteAsync(Guid projectId, CancellationToken cancellationToken = default)
		{
			if (projectId == Guid.Empty)
			{
				_logger.LogWarning("DeleteAsync called with empty ProjectId");
				throw new ArgumentException("Project ID cannot be empty.", nameof(projectId));
			}

			try
			{
				_logger.LogDebug("Deleting project with ID {ProjectId}", projectId);

				var projectEntity = await _context.Projects
					.Include(p => p.Attachments)
					.Include(p => p.Milestones)
					.FirstOrDefaultAsync(p => p.Id == projectId, cancellationToken);

				if (projectEntity == null)
				{
					_logger.LogWarning("Project with ID {ProjectId} not found for deletion", projectId);
					throw new KeyNotFoundException($"Project with ID {projectId} not found.");
				}

				_context.Projects.Remove(projectEntity);
				_logger.LogInformation("Project with ID {ProjectId} deleted successfully", projectId);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to delete project with ID: {ProjectId}", projectId);
				throw;
			}
		}



		public async Task UpdateStatusAsync(Guid projectId, ProjectStatus status, CancellationToken cancellationToken = default)
		{
			if (projectId == Guid.Empty)
			{
				_logger.LogWarning("UpdateStatusAsync called with empty ProjectId");
				throw new ArgumentException("Project ID cannot be empty.", nameof(projectId));
			}

			try
			{
				_logger.LogDebug("Updating status for project {ProjectId} to {Status}", projectId, status);

				var rowsAffected = await _context.Projects
					.Where(p => p.Id == projectId)
					.ExecuteUpdateAsync(p => p.SetProperty(x => x.Status, (int)status), cancellationToken);

				if (rowsAffected == 0)
				{
					_logger.LogWarning("Project with ID {ProjectId} not found for status update", projectId);
					throw new KeyNotFoundException($"Project with ID {projectId} not found.");
				}

				_logger.LogInformation("Status updated for project {ProjectId} to {Status}", projectId, status);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to update status for project {ProjectId}", projectId);
				throw;
			}
		}

		public async Task UpdateTitleAsync(Guid projectId, string title, CancellationToken cancellationToken = default)
		{
			if (projectId == Guid.Empty)
			{
				_logger.LogWarning("UpdateTitleAsync called with empty ProjectId");
				throw new ArgumentException("Project ID cannot be empty.", nameof(projectId));
			}

			try
			{
				_logger.LogDebug("Updating title for project {ProjectId}", projectId);

				var rowsAffected = await _context.Projects
					.Where(p => p.Id == projectId)
					.ExecuteUpdateAsync(p => p.SetProperty(x => x.Title, title), cancellationToken);

				if (rowsAffected == 0)
				{
					_logger.LogWarning("Project with ID {ProjectId} not found for title update", projectId);
					throw new KeyNotFoundException($"Project with ID {projectId} not found.");
				}

				_logger.LogInformation("Title updated for project {ProjectId}", projectId);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to update title for project {ProjectId}", projectId);
				throw;
			}
		}

		public async Task UpdateDescriptionAsync(Guid projectId, string description, CancellationToken cancellationToken = default)
		{
			if (projectId == Guid.Empty)
			{
				_logger.LogWarning("UpdateDescriptionAsync called with empty ProjectId");
				throw new ArgumentException("Project ID cannot be empty.", nameof(projectId));
			}

			try
			{
				_logger.LogDebug("Updating description for project {ProjectId}", projectId);

				var rowsAffected = await _context.Projects
					.Where(p => p.Id == projectId)
					.ExecuteUpdateAsync(p => p.SetProperty(x => x.Description, description), cancellationToken);

				if (rowsAffected == 0)
				{
					_logger.LogWarning("Project with ID {ProjectId} not found for description update", projectId);
					throw new KeyNotFoundException($"Project with ID {projectId} not found.");
				}

				_logger.LogInformation("Description updated for project {ProjectId}", projectId);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to update description for project {ProjectId}", projectId);
				throw;
			}
		}

		public async Task UpdateBudgetAsync(Guid projectId, decimal budgetMin, decimal budgetMax, string currencyCode, CancellationToken cancellationToken = default)
		{
			if (projectId == Guid.Empty)
			{
				_logger.LogWarning("UpdateBudgetAsync called with empty ProjectId");
				throw new ArgumentException("Project ID cannot be empty.", nameof(projectId));
			}

			try
			{
				_logger.LogDebug("Updating budget for project {ProjectId}", projectId);

				var rowsAffected = await _context.Projects
					.Where(p => p.Id == projectId)
					.ExecuteUpdateAsync(p => p
						.SetProperty(x => x.BudgetMin, budgetMin)
						.SetProperty(x => x.BudgetMax, budgetMax)
						.SetProperty(x => x.CurrencyCode, currencyCode), cancellationToken);

				if (rowsAffected == 0)
				{
					_logger.LogWarning("Project with ID {ProjectId} not found for budget update", projectId);
					throw new KeyNotFoundException($"Project with ID {projectId} not found.");
				}

				_logger.LogInformation("Budget updated for project {ProjectId}", projectId);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to update budget for project {ProjectId}", projectId);
				throw;
			}
		}

		public async Task UpdateTagsAsync(Guid projectId, List<string> tags, CancellationToken cancellationToken = default)
		{
			if (projectId == Guid.Empty)
			{
				_logger.LogWarning("UpdateTagsAsync called with empty ProjectId");
				throw new ArgumentException("Project ID cannot be empty.", nameof(projectId));
			}

			try
			{
				_logger.LogDebug("Updating tags for project {ProjectId}", projectId);

				string tagsString = string.Join(",", tags);

				var rowsAffected = await _context.Projects
					.Where(p => p.Id == projectId)
					.ExecuteUpdateAsync(p => p.SetProperty(x => x.Tags, tagsString), cancellationToken);

				if (rowsAffected == 0)
				{
					_logger.LogWarning("Project with ID {ProjectId} not found for tags update", projectId);
					throw new KeyNotFoundException($"Project with ID {projectId} not found.");
				}

				_logger.LogInformation("Tags updated for project {ProjectId}", projectId);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to update tags for project {ProjectId}", projectId);
				throw;
			}
		}

		private async Task SyncAttachmentsAsync(ProjectEntity projectEntity, IEnumerable<ProjectAttachment> attachments,
			CancellationToken cancellationToken)
		{
			var attachmentsList = attachments?.ToList() ?? new List<ProjectAttachment>();
			var existingDict = projectEntity.Attachments.ToDictionary(e => e.Id);
			var newDict = attachmentsList.ToDictionary(a => a.Id);

			var toRemove = projectEntity.Attachments.Where(e => !newDict.ContainsKey(e.Id)).ToList();
			foreach (var attachment in toRemove)
			{
				projectEntity.Attachments.Remove(attachment);
			}

			foreach (var attachment in attachmentsList)
			{
				if (existingDict.TryGetValue(attachment.Id, out var existingEntity))
				{
					_mapper.Map(attachment, existingEntity);
				}
				else
				{
					var newEntity = _mapper.Map<ProjectAttachmentEntity>(attachment);
					newEntity.ProjectId = projectEntity.Id;
					_context.Entry(newEntity).State = EntityState.Added;
					projectEntity.Attachments.Add(newEntity);
				}
			}
		}

		private async Task SyncMilestonesAsync(ProjectEntity projectEntity, IEnumerable<ProjectMilestone> milestones,
			CancellationToken cancellationToken)
		{
			var milestonesList = milestones?.ToList() ?? new List<ProjectMilestone>();
			var existingDict = projectEntity.Milestones.ToDictionary(e => e.Id);
			var newDict = milestonesList.ToDictionary(m => m.Id);

			var toRemove = projectEntity.Milestones.Where(e => !newDict.ContainsKey(e.Id)).ToList();
			foreach (var milestone in toRemove)
			{
				projectEntity.Milestones.Remove(milestone);
			}

			foreach (var milestone in milestonesList)
			{
				if (existingDict.TryGetValue(milestone.Id, out var existingEntity))
				{
					_mapper.Map(milestone, existingEntity);
				}
				else
				{
					var newEntity = _mapper.Map<ProjectMilestoneEntity>(milestone);
					newEntity.ProjectId = projectEntity.Id;
					_context.Entry(newEntity).State = EntityState.Added;
					projectEntity.Milestones.Add(newEntity);
				}
			}
		}
	}
}