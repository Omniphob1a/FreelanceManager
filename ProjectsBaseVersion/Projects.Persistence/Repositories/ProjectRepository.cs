using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Projects.Domain.Entities;
using Projects.Domain.Enums;
using Projects.Domain.Repositories;
using Projects.Persistence.Data;
using Projects.Persistence.Models;
using Projects.Shared.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projects.Persistence.Repositories
{
	public class ProjectRepository : IProjectRepository
	{
		private readonly ProjectsDbContext _context;
		private readonly IMapper _mapper;
		private readonly ILogger<ProjectRepository> _logger;

		public ProjectRepository(IMapper mapper, ProjectsDbContext context, ILogger<ProjectRepository> logger)
		{
			_mapper = mapper;
			_context = context;
			_logger = logger;
		}

		public async Task AddAsync(Project project, CancellationToken cancellationToken)
		{
			if (project == null)
			{
				_logger.LogWarning("Trying to add null project");
				throw new ArgumentNullException(nameof(project));
			}

			ProjectEntity projectData;

			try
			{
				projectData = _mapper.Map<ProjectEntity>(project);
			}
			catch (Exception ex)
			{
				_logger.LogMappingError<Project, ProjectEntity>(project, ex);
				throw;
			}

			try
			{
				await _context.Projects.AddAsync(projectData, cancellationToken);
				_logger.LogInformation("Project with ID {ProjectId} added successfully", project.Id);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Failed to persist Project with ID {ProjectId}", project.Id);
				throw;
			}
		}

		public async Task DeleteAsync(Guid projectId, CancellationToken cancellationToken)
		{
			if (projectId == Guid.Empty)
			{
				_logger.LogWarning("DeleteAsync called with empty ProjectId");
				throw new ArgumentException("Project ID cannot be empty.", nameof(projectId));
			}

			try
			{
				_logger.LogInformation("Trying to delete project with ID {ProjectId}", projectId);

				var project = await _context.Projects
					.Include(p => p.Attachments)       
					.Include(p => p.Milestones)   
					.FirstOrDefaultAsync(p => p.Id == projectId, cancellationToken);

				if (project == null)
				{
					_logger.LogWarning("Project with ID {ProjectId} not found", projectId);
					throw new KeyNotFoundException($"Project with ID {projectId} not found.");
				}

				_context.ProjectAttachments.RemoveRange(project.Attachments);
				_context.ProjectMilestones.RemoveRange(project.Milestones);

				_context.Projects.Remove(project);

				_logger.LogInformation("Project with ID {ProjectId} successfully deleted", projectId);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to delete project with ID: {ProjectId}", projectId);
				throw;
			}
		}

		public async Task<bool> ExistsAsync(Guid projectId, CancellationToken cancellationToken)
		{
			if (projectId == Guid.Empty)
			{
				_logger.LogWarning("ExistsAsync called with empty ProjectId");
				throw new ArgumentException("Project ID cannot be empty.", nameof(projectId));
			}

			try
			{
				_logger.LogInformation("Checking if project with ID {ProjectId} exists", projectId);

				var exists = await _context.Projects
					.AsNoTracking()
					.AnyAsync(p => p.Id == projectId, cancellationToken);

				_logger.LogInformation("Project with ID {ProjectId} exists: {Exists}", projectId, exists);

				return exists;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to check existence of project with ID: {ProjectId}", projectId);
				throw;
			}
		}

		public async Task<IEnumerable<Project>> GetActiveAsync(CancellationToken cancellationToken)
		{
			try
			{
				_logger.LogInformation("Trying to get active projects");

				var entities = await _context.Projects
					.AsNoTracking()
					.Where(p => p.Status == (int)ProjectStatus.Active)
					.ToListAsync(cancellationToken);

				try
				{
					var projects = _mapper.Map<List<Project>>(entities);
					_logger.LogInformation("Successfully mapped {Count} active projects", projects.Count);
					return projects;
				}
				catch (Exception ex)
				{
					_logger.LogMappingError<ProjectEntity, Project>(entities, ex);
					throw; 
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to get active projects from database");
				throw;
			}
		}

		public async Task<Project?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
		{
			if (id == Guid.Empty)
			{
				_logger.LogError("Invalid project ID: Guid.Empty");
				throw new ArgumentException("Invalid project ID: Guid.Emptymessage", nameof(id));
			}

			try
			{
				_logger.LogInformation("Trying to get project by filter (id) : {id}", id);
				var entity = await _context.Projects
					.AsNoTracking()
					.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

				if (entity is null)
				{
					_logger.LogWarning("No project found with ID: {ProjectId}", id);
					return null;
				}

				try
				{
					return _mapper.Map<Project>(entity);
				}
				catch (Exception ex)
				{
					_logger.LogMappingError<ProjectEntity, Project>(entity, ex);
					throw;
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to get project with id: {ProjectId}", id);
				throw;
			}
		}

		public async Task<List<Project>> GetByOwnerIdAsync(Guid ownerId, CancellationToken cancellationToken)
		{
			if (ownerId == Guid.Empty)
			{
				_logger.LogWarning("GetByOwnerIdAsync called with empty OwnerId");
				throw new ArgumentException("Owner ID cannot be empty.", nameof(ownerId));
			}

			try
			{
				_logger.LogInformation("Retrieving projects by owner ID: {OwnerId}", ownerId);

				var entities = await _context.Projects
					.AsNoTracking()
					.Where(p => p.OwnerId == ownerId)
					.ToListAsync(cancellationToken);

				try
				{
					var projects = _mapper.Map<List<Project>>(entities);
					_logger.LogInformation("Mapped {Count} projects for OwnerId: {OwnerId}", projects.Count, ownerId);
					return projects;
				}
				catch (Exception ex)
				{
					_logger.LogMappingError<ProjectEntity, Project>(entities, ex);
					throw;
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to get projects by OwnerId: {OwnerId}", ownerId);
				throw;
			}
		}
	}
}
