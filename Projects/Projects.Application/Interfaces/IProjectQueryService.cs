using Projects.Application.Common.Filters;
using Projects.Application.Common.Pagination;
using Projects.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projects.Application.Interfaces
{
	public interface IProjectQueryService
	{
		Task<PaginatedResult<Project>> GetPaginatedAsync(ProjectFilter filter, CancellationToken ct);
		Task<Project?> GetByIdAsync(Guid id, CancellationToken ct);
		Task<Project?> GetByIdWithMilestonesAsync(Guid id, CancellationToken ct);
		Task<Project?> GetByIdWithAttachmentsAsync(Guid id, CancellationToken ct);
		Task<Project> GetFullProjectByIdAsync(Guid id, CancellationToken ct);
		Task<List<Project>> GetOutOfDateProjectsAsync(DateTime thresholdDate);
		Task<List<Project>> GetAllAsync();
	}
}
