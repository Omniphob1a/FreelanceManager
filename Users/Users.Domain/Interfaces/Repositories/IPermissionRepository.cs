using Users.Domain.ValueObjects;

namespace Users.Domain.Interfaces.Repositories
{
	public interface IPermissionRepository
	{
		Task<Permission?> GetById(Guid id, CancellationToken cancellationToken);
		Task<IEnumerable<Permission>> GetAll(CancellationToken cancellationToken);
		Task Add(Permission permission, CancellationToken cancellationToken);
		Task Update(Permission permission, CancellationToken cancellationToken);
		Task Delete(Guid id, CancellationToken cancellationToken);
		Task<IEnumerable<string>> GetPermissionsByRoleIds(IEnumerable<Guid> roleIds, CancellationToken cancellationToken);
	}

}
