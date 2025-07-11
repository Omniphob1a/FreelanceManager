using Projects.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projects.Domain.Repositories
{
	public interface IProjectRepository
	{
		Task<Project?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
		Task AddAsync(Project project, CancellationToken cancellationToken);
		Task<IEnumerable<Project>> GetActiveAsync(CancellationToken cancellationToken);
		Task<bool> ExistsAsync(Guid projectId, CancellationToken cancellationToken);
		Task<List<Project>> GetByOwnerIdAsync(Guid ownerId, CancellationToken cancellationToken);
		Task DeleteAsync(Guid id, CancellationToken cancellationToken);
	}
}
