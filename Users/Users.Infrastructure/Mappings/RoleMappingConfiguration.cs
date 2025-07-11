using Mapster;
using Users.Domain.Entities;
using Users.Infrastructure.Models;

public class RoleMappingConfiguration : IRegister
{
	public void Register(TypeAdapterConfig config)
	{
		config.NewConfig<RoleEntity, Role>()
			.MapWith(src => MapFromEntity(src))
			.IgnoreNonMapped(true);

		config.NewConfig<Role, RoleEntity>()
			.Map(dest => dest.Id, src => src.Id)
			.Map(dest => dest.Name, src => src.Name)
			.Map(dest => dest.RolePermissions,
				src => src.PermissionIds.Select(pid => new RolePermissionEntity
				{
					RoleId = src.Id,
					PermissionId = pid
				}))
			.IgnoreNonMapped(true);
	}

	private static Role MapFromEntity(RoleEntity src)
	{
		var roleResult = Role.TryCreate(src.Name);
		if (!roleResult.IsSuccess)
			throw new InvalidDataException($"Invalid role: {string.Join(", ", roleResult.Errors)}");

		var role = roleResult.Value;

		typeof(Role)
			.GetProperty("Id")?
			.SetValue(role, src.Id);

		foreach (var permissionId in src.RolePermissions.Select(rp => rp.PermissionId))
		{
			role.AddPermission(permissionId);
		}

		return role;
	}
}