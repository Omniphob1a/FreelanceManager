using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tasks.Application.DTOs;

namespace Tasks.Application.Interfaces
{
	public interface IProjectService
	{
		Task<bool> ExistsAsync(Guid projectId, CancellationToken cancellationToken);
		Task<ProjectDto?> GetProjectByIdAsync(Guid projectId, CancellationToken cancellationToken);
	}
}
