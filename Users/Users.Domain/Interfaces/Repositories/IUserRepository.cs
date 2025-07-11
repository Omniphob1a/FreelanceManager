using Users.Domain.Entities;
using Users.Domain.ValueObjects;

namespace Users.Domain.Interfaces.Repositories
{
	public interface IUserRepository
	{
		Task Add(User user, CancellationToken cancellationToken);
		Task<User> GetByEmail(string email, CancellationToken cancellationToken);
		Task<User> GetByLogin(string login, CancellationToken cancellationToken);
		Task<User> GetById(Guid id, CancellationToken cancellationToken);
		Task<IEnumerable<User>> ListActive(CancellationToken cancellationToken);
		Task<IEnumerable<User>> ListByAge(int minAge, CancellationToken cancellationToken);
		Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken);
		Task Delete(Guid id, CancellationToken cancellationToken);
		Task Update(User user, CancellationToken cancellationToken);
		Task<List<string>> GetUserPermissions(Guid userId, CancellationToken cancellationToken);
		Task<List<string>> GetUserRoles(Guid userId, CancellationToken cancellationToken);
	}
}
