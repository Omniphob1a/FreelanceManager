using Mapster;
using Users.Domain.Entities;
using Users.Domain.ValueObjects;
using Users.Infrastructure.Models;

public class UserMappingConfiguration : IRegister
{
	public void Register(TypeAdapterConfig config)
	{
		config.NewConfig<User, UserData>()
			.Map(dest => dest.Id, src => src.Id)
			.Map(dest => dest.Login, src => src.Login)
			.Map(dest => dest.PasswordHash, src => src.PasswordHash)
			.Map(dest => dest.Name, src => src.Name)
			.Map(dest => dest.Gender, src => src.Gender)
			.Map(dest => dest.Birthday, src => src.Birthday)
			.Map(dest => dest.Admin, src => src.Admin)
			.Map(dest => dest.Email, src => src.Email.Value)
			.Map(dest => dest.CreatedAt, src => src.CreatedAt)
			.Map(dest => dest.CreatedBy, src => src.CreatedBy)
			.Map(dest => dest.ModifiedOn, src => src.ModifiedOn)
			.Map(dest => dest.ModifiedBy, src => src.ModifiedBy)
			.Map(dest => dest.RevokedOn, src => src.RevokedOn)
			.Map(dest => dest.RevokedBy, src => src.RevokedBy)
			.Map(dest => dest.UserRoles,
				src => src.RoleIds.Select(roleId => new UserRoleEntity { UserId = src.Id, RoleId = roleId }))
			.IgnoreNonMapped(true);

		config.NewConfig<UserData, User>()
			.Map(dest => dest.Id, src => src.Id)
			.MapWith(src => MapUserFromData(src))
			.IgnoreNonMapped(true);
	}

	private static User MapUserFromData(UserData src)
	{
		var userResult = User.TryCreate(
			src.Login,
			src.PasswordHash,
			src.Name,
			src.Gender,
			src.Birthday,
			new Email(src.Email),
			src.CreatedBy,
			src.Admin
		);

		if (!userResult.IsSuccess)
			throw new InvalidDataException($"Invalid user data: {string.Join(", ", userResult.Errors)}");

		var user = userResult.Value;

		typeof(User)
			.GetProperty("Id")?
			.SetValue(user, src.Id);

		SetPropertyIfExists(user, "ModifiedOn", src.ModifiedOn);
		SetPropertyIfExists(user, "ModifiedBy", src.ModifiedBy);
		SetPropertyIfExists(user, "RevokedOn", src.RevokedOn);
		SetPropertyIfExists(user, "RevokedBy", src.RevokedBy);

		foreach (var roleId in src.UserRoles.Select(ur => ur.RoleId))
		{
			user.AssignRole(roleId);
		}

		return user;
	}

	private static void SetPropertyIfExists<T>(User user, string propName, T value)
	{
		var prop = typeof(User).GetProperty(propName);
		if (prop?.GetSetMethod(nonPublic: true) != null && value != null)
		{
			prop.SetValue(user, value);
		}
	}
}