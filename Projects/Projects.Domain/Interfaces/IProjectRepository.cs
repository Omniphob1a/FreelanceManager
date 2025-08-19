using Projects.Domain.Entities;
using Projects.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projects.Domain.Repositories
{
	public interface IProjectRepository
	{
		Task<bool> ExistsAsync(Guid projectId, CancellationToken cancellationToken = default);
		Task AddAsync(Project project, CancellationToken cancellationToken = default);
		Task UpdateAsync(Project project, CancellationToken cancellationToken = default);
		Task DeleteAsync(Guid projectId, CancellationToken cancellationToken = default);

		Task UpdateStatusAsync(Guid projectId, ProjectStatus status, CancellationToken cancellationToken = default);
		Task UpdateTitleAsync(Guid projectId, string title, CancellationToken cancellationToken = default);
		Task UpdateDescriptionAsync(Guid projectId, string description, CancellationToken cancellationToken = default);
		Task UpdateBudgetAsync(Guid projectId, decimal budgetMin, decimal budgetMax, string currencyCode, CancellationToken cancellationToken = default);
		Task UpdateTagsAsync(Guid projectId, List<string> tags, CancellationToken cancellationToken = default);
	}
}
