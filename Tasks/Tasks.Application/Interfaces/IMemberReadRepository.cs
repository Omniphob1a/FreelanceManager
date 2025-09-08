using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tasks.Application.DTOs;

namespace Tasks.Application.Interfaces
{
	public interface IMemberReadRepository
	{
		Task<List<MemberDto>> GetByProjectAsync(Guid projectId, CancellationToken ct);
		Task<bool> ExistsAsync(Guid projectId, Guid userId, CancellationToken ct);
	}
}
