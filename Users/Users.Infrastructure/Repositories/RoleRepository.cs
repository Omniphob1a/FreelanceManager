using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using Users.Domain.Entities;
using Users.Domain.Interfaces.Repositories;
using Users.Infrastructure.Data;
using Users.Infrastructure.Models;

namespace Users.Infrastructure.Repositories
{
	public class RoleRepository : IRoleRepository
	{
		private readonly UsersDbContext _context;
		private readonly IMapper _mapper;

		public RoleRepository(UsersDbContext context, IMapper mapper)
		{
			_context = context;
			_mapper = mapper;
		}

		public async Task Add(Role role, CancellationToken cancellationToken)
		{
			var roleEntity = _mapper.Map<RoleEntity>(role);
			await _context.Roles.AddAsync(roleEntity, cancellationToken);
			await _context.SaveChangesAsync(cancellationToken);
		}

		public async Task Delete(Guid roleId, CancellationToken cancellationToken)
		{
			var entity = await _context.Roles
				.FirstOrDefaultAsync(p => p.Id == roleId, cancellationToken);

			if (entity == null)
				throw new InvalidOperationException($"Role with ID {roleId} not found.");

			_context.Roles.Remove(entity);
			await _context.SaveChangesAsync(cancellationToken);
		}

		public async Task<Role?> GetById(Guid roleId, CancellationToken cancellationToken)
		{
			var roleEntity = await _context.Roles
				.Include(r => r.RolePermissions)
				.FirstOrDefaultAsync(r => r.Id == roleId, cancellationToken);

			return roleEntity == null
				? null
				: _mapper.Map<Role>(roleEntity);
		}

		public async Task<Role?> GetByName(string name, CancellationToken cancellationToken)
		{
			var roleEntity = await _context.Roles
				.Include(r => r.RolePermissions)
				.FirstOrDefaultAsync(r => r.Name == name, cancellationToken);

			return roleEntity == null
				? null
				: _mapper.Map<Role>(roleEntity);
		}

		public async Task<IReadOnlyList<Role>> ListAll(CancellationToken cancellationToken)
		{
			var entities = await _context.Roles
				.AsNoTracking()
				.Include(r => r.RolePermissions)
				.ToListAsync(cancellationToken);

			var roles = entities
				.Select(e => _mapper.Map<Role>(e))
				.ToList();

			return roles;
		}

		public async Task Update(Role role, CancellationToken cancellationToken)
		{
			var existingEntity = await _context.Roles
				.Include(r => r.RolePermissions)
				.FirstOrDefaultAsync(r => r.Id == role.Id, cancellationToken);

			if (existingEntity == null)
				throw new KeyNotFoundException("Role not found");

			var updatedEntity = _mapper.Map<RoleEntity>(role);
			_context.Entry(existingEntity).CurrentValues.SetValues(updatedEntity);

			existingEntity.RolePermissions.Clear();
			foreach (var permissionId in role.PermissionIds)
			{
				existingEntity.RolePermissions.Add(new RolePermissionEntity
				{
					RoleId = role.Id,
					PermissionId = permissionId
				});
			}

			await _context.SaveChangesAsync(cancellationToken);
		}

		public async Task<List<string>> GetRoleNamesByIds(IEnumerable<Guid> roleIds, CancellationToken ct)
		{
			return await _context.Roles
				.Where(r => roleIds.Contains(r.Id))
				.Select(r => r.Name)
				.ToListAsync(ct);
		}
	}
}
