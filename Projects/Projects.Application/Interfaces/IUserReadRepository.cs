using Projects.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projects.Application.Interfaces
{
	public interface IUserReadRepository
	{
		Task<PublicUserDto?> GetByIdAsync(Guid userId, CancellationToken ct);
		Task<List<PublicUserDto>> GetByIdsAsync(IEnumerable<Guid> userIds, CancellationToken ct);
		Task<PublicUserDto?> GetByLoginAsync(string login, CancellationToken ct);
		Task<List<PublicUserDto>> GetAllAsync(CancellationToken ct);
		Task<bool> ExistsAsync(Guid userId, CancellationToken ct);
	}
}
