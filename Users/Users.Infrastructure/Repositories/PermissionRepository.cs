using FluentResults;
using Microsoft.EntityFrameworkCore;
using Users.Domain.Entities;
using Users.Domain.Interfaces.Repositories;
using Users.Infrastructure.Data;
using Users.Infrastructure.Models;
using Mapster;
using Users.Domain.ValueObjects;

namespace Users.Infrastructure.Repositories;

public class PermissionRepository : IPermissionRepository
{
	private readonly UsersDbContext _context;

	public PermissionRepository(UsersDbContext context)
	{
		_context = context;
	}

	public async Task Add(Permission permission, CancellationToken cancellationToken)
	{
		var permissionEntity = permission.Adapt<PermissionEntity>();
		await _context.Permissions.AddAsync(permissionEntity, cancellationToken);
		await _context.SaveChangesAsync(cancellationToken);
	}

	public async Task<Permission?> GetById(Guid id, CancellationToken cancellationToken)
	{
		var entity = await _context.Permissions
			.AsNoTracking()
			.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

		return entity?.Adapt<Permission>();
	}

	public async Task<IEnumerable<Permission>> GetAll(CancellationToken cancellationToken)
	{
		var entities = await _context.Permissions
			.AsNoTracking()
			.ToListAsync(cancellationToken);
		return entities.Adapt<List<Permission>>();
	}

	public async Task Update(Permission permission, CancellationToken cancellationToken)
	{
		var entity = await _context.Permissions
			.FirstOrDefaultAsync(p => p.Name == permission.Name, cancellationToken);

		if (entity == null)
			throw new KeyNotFoundException("Permission not found");

		permission.Adapt(entity);
		await _context.SaveChangesAsync(cancellationToken);
	}

	public async Task Delete(Guid id, CancellationToken cancellationToken)
	{
		var entity = await _context.Permissions
			.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

		if (entity == null)
			throw new InvalidOperationException($"Permission with ID {id} not found.");

		_context.Permissions.Remove(entity);
		await _context.SaveChangesAsync(cancellationToken);
	}

	public async Task<IEnumerable<string>> GetPermissionsByRoleIds(IEnumerable<Guid> roleIds, CancellationToken ct)
	{
		return await _context.RolePermissions
			.Where(rp => roleIds.Contains(rp.RoleId))
			.Select(rp => rp.Permission.Name)
			.Distinct()
			.ToListAsync(ct);
	}

}
