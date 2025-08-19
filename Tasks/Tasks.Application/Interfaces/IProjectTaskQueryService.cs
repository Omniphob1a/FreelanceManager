using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tasks.Application.Common.Filters;
using Tasks.Application.Common.Pagination;
using Tasks.Domain.Aggregate.Root;

namespace Tasks.Application.Interfaces
{
	public interface IProjectTaskQueryService
	{
		Task<PaginatedResult<ProjectTask>> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken);
		Task<PaginatedResult<ProjectTask>> GetByAssigneeIdAsync(Guid assigneeId, TaskFilter filter, PaginationInfo paginationInfo, CancellationToken cancellationToken);
		Task<PaginatedResult<ProjectTask>> GetPaginatedAsync(CancellationToken cancellationToken);
	}
}
