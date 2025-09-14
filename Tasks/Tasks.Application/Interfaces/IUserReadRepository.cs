using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tasks.Application.DTOs;

namespace Tasks.Application.Interfaces
{
	public interface IUserReadRepository
	{
		Task<PublicUserDto?> GetByIdAsync(Guid userId, CancellationToken ct);
		Task<List<PublicUserDto>> GetByIdsAsync(IEnumerable<Guid> userIds, CancellationToken ct);
		Task<List<PublicUserDto>> GetAllAsync(CancellationToken ct);
		Task<bool> ExistsAsync(Guid userId, CancellationToken ct);
	}
}
