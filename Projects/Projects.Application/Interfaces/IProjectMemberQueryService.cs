using Projects.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projects.Application.Interfaces
{
	public interface IProjectMemberQueryService
	{
		Task<List<ProjectMemberReadDto>> GetProjectMembersAsync(Guid projectId, CancellationToken ct);
	}
}
