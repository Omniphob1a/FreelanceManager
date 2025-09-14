using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Projects.Application.DTOs;
using Projects.Application.Interfaces;
using Projects.Persistence.Models.ReadModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projects.Persistence.Data.Queries
{
	public class ProjectMemberQueryService : IProjectMemberQueryService
	{
		private readonly ILogger<ProjectMemberQueryService> _logger;
		private readonly ProjectsDbContext _context;
		private readonly IUserReadRepository _userReadRepository;

		public ProjectMemberQueryService(ILogger<ProjectMemberQueryService> logger, ProjectsDbContext context, IUserReadRepository userReadRepository) 
		{ 
			_logger = logger; 
			_context = context;
			_userReadRepository = userReadRepository;
		}

		public async Task<List<ProjectMemberReadDto>> GetProjectMembersAsync(Guid projectId, CancellationToken ct)
		{
			_logger.LogDebug("Getting members for project {ProjectId}", projectId);

			try
			{
				var members = await _context.ProjectMembers
					.AsNoTracking()
					.Where(x => x.ProjectId == projectId)
					.ToListAsync(ct);

				_logger.LogDebug("Found {Count} project members for {ProjectId}", members.Count, projectId);

				if (!members.Any())
				{
					_logger.LogInformation("No members found for project {ProjectId}", projectId);
					return new List<ProjectMemberReadDto>();
				}

				var userIds = members.Select(m => m.UserId).ToList();
				var users = await _userReadRepository.GetByIdsAsync(userIds, ct);

				_logger.LogDebug("Loaded {Count} users for project {ProjectId}", users.Count, projectId);

				var dtos = members.Select(m =>
				{
					var user = users.FirstOrDefault(u => u.Id == m.UserId);
					if (user == null)
					{
						_logger.LogWarning("No user found for ProjectMember {MemberId} (UserId={UserId}) in project {ProjectId}",
							m.Id, m.UserId, projectId);
					}

					return new ProjectMemberReadDto
					{
						Id = m.Id,
						ProjectId = m.ProjectId,
						Role = m.Role,
						AddedAt = m.AddedAt,
						User = user
					};
				}).ToList();

				_logger.LogDebug("Returning {Count} member DTOs for project {ProjectId}", dtos.Count, projectId);

				return dtos;
			}
			catch (OperationCanceledException)
			{
				_logger.LogWarning("GetProjectMembersAsync was canceled for project {ProjectId}", projectId);
				throw; 
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to get members for project {ProjectId}", projectId);
				return new List<ProjectMemberReadDto>();
			}
		}

	}
}
