using FluentResults;
using Users.Domain.ValueObjects;

namespace Users.Domain.Entities;

public class Role
{
	public Guid Id { get; private set; }
	public string Name { get; private set; }
	public ICollection<Guid> PermissionIds { get; private set; } = new List<Guid>();

	private Role() { }

	public Role(string name)
	{
		Id = Guid.NewGuid();
		Name = name;
	}

	public static Result<Role> TryCreate(string name)
	{
		var validationResult = Validate(name);
		if (validationResult.IsFailed)
			return Result.Fail(validationResult.Errors);

		return new Role
		{
			Id = Guid.NewGuid(),
			Name = name
		};
	}

	public void AddPermission(Guid permissionId)
	{
		if (!PermissionIds.Contains(permissionId))
			PermissionIds.Add(permissionId);
	}

	public void RemovePermission(Guid permissionId)
		=> PermissionIds.Remove(permissionId);

	private static Result Validate(string name)
	{
		if (string.IsNullOrWhiteSpace(name))
			return Result.Fail("Role name cannot be empty");

		if (name.Length > 50)
			return Result.Fail("Role name too long (max 50 chars)");

		return Result.Ok();
	}
}
