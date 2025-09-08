using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tasks.Application.DTOs;

namespace Tasks.Application.Interfaces
{
	public interface IProjectReadRepository
	{
		Task<bool> ExistsAsync(Guid projectId, CancellationToken ct);
		Task<ProjectDto?> GetByIdAsync(Guid projectId, CancellationToken ct);
		Task<List<ProjectDto>> GetByOwnerAsync(Guid ownerId, CancellationToken ct);
		Task<List<ProjectDto>> GetAllAsync(CancellationToken ct);
	}
}
