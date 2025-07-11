using Users.Domain.Entities;

namespace Users.Domain.Interfaces.Repositories
{
	public interface IRoleRepository
	{
		Task Add(Role role, CancellationToken cancellationToken);
		Task<Role?> GetById(Guid id, CancellationToken cancellationToken);
		Task<Role?> GetByName(string name, CancellationToken cancellationToken);
		Task Update(Role role, CancellationToken cancellationToken);
		Task Delete(Guid roleId, CancellationToken cancellationToken);
		Task<IReadOnlyList<Role>> ListAll(CancellationToken cancellationToken);
		Task<List<string>> GetRoleNamesByIds(IEnumerable<Guid> roleIds, CancellationToken cancellationToken);
	}
}
