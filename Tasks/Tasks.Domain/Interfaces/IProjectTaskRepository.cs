using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tasks.Domain.Aggregate.Root;

namespace Tasks.Domain.Interfaces
{
	public interface IProjectTaskRepository
	{
		Task<ProjectTask> GetByIdAsync(Guid id, CancellationToken cancellationToken);
		Task<ProjectTask> GetFullTaskByIdAsync(Guid id, CancellationToken cancellationToken);
		Task AddAsync(ProjectTask task, CancellationToken cancellationToken);
		Task UpdateAsync(ProjectTask task, CancellationToken cancellationToken);
		Task DeleteAsync(Guid TaskId, CancellationToken cancellationToken);
	}
}
