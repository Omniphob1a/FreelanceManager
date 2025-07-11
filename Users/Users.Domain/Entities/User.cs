using FluentResults;
using System.Security.Cryptography;
using System.Text;
using Users.Domain.ValueObjects;

namespace Users.Domain.Entities;
public class User
{
	public Guid Id { get; private set; }
	public string Login { get; private set; }
	public string PasswordHash { get; private set; }
	public string Name { get; private set; }
	public int Gender { get; private set; }
	public DateTime? Birthday { get; private set; }
	public bool Admin { get; private set; }
	public Email Email { get; private set; }
	public DateTime CreatedAt { get; private set; }
	public string CreatedBy { get; private set; }
	public DateTime? ModifiedOn { get; private set; }
	public string ModifiedBy { get; private set; }
	public DateTime? RevokedOn { get; private set; }
	public string RevokedBy { get; private set; }
	public ICollection<Guid> RoleIds { get; set; } = [];

	private User() { }

	private static Result Validate(string login, string passwordHash, string name, int gender, DateTime? birthday, Email email)
	{
		var result = Result.Ok();

		if (string.IsNullOrWhiteSpace(login))
			result = result.WithError("Login cannot be empty");

		if (string.IsNullOrWhiteSpace(passwordHash))
			result = result.WithError("Password hash cannot be empty");

		if (string.IsNullOrWhiteSpace(name))
			result = result.WithError("Name cannot be empty");

		if (email is null)
			result = result.WithError("Email cannot be null");

		if (gender < 0 || gender > 2)
			result = result.WithError("Gender must be 0 (Unknown), 1 (Male), or 2 (Female)");

		if (birthday > DateTime.UtcNow)
			result = result.WithError("Birthday cannot be in the future");

		return result;
	}

	private static Result ValidateUserData(string name, int gender, DateTime? birthday, Email email)
	{
		var result = Result.Ok();

		if (string.IsNullOrWhiteSpace(name))
			result = result.WithError("Name cannot be empty");

		if (email is null)
			result = result.WithError("Email cannot be null");

		if (gender < 0 || gender > 2)
			result = result.WithError("Gender must be 0 (Unknown), 1 (Male), or 2 (Female)");

		if (birthday > DateTime.UtcNow)
			result = result.WithError("Birthday cannot be in the future");

		return result;
	}

	public static Result<User> TryCreate(
		string login,
		string passwordHash,
		string name,
		int gender,
		DateTime? birthday,
		Email email,
		string createdBy,
		bool isAdmin)
	{
		var validationResult = Validate(login, passwordHash, name, gender, birthday, email);

		if (validationResult.IsFailed)
		{
			return Result.Fail(validationResult.Errors);
		}

		var user = new User
		{
			Id = Guid.NewGuid(),
			Login = login,
			PasswordHash = passwordHash,
			Name = name,
			Gender = gender,
			Birthday = birthday,
			Email = email,
			CreatedAt = DateTime.UtcNow,
			CreatedBy = createdBy,
			Admin = isAdmin
		};

		return Result.Ok(user);
	}

	public Result AssignRole(Guid roleId)
	{
		if (!RoleIds.Contains(roleId))
		{
			RoleIds.Add(roleId);
			return Result.Ok();
		}
		return Result.Fail("Role already assigned.");
	}

	public Result RemoveRole(Guid roleId)
	{
		if (RoleIds.Contains(roleId))
		{
			RoleIds.Remove(roleId);
			return Result.Ok();
		}
		return Result.Fail("Role not found.");
	}
	public Result UpdateName(string newName, string modifiedBy)
	{
		if (string.IsNullOrWhiteSpace(newName))
			return Result.Fail("Password hash cannot be empty.");

		Name = newName;
		ModifiedOn = DateTime.UtcNow;
		ModifiedBy = modifiedBy;

		return Result.Ok();
	}

	public Result UpdateLogin(string newLogin, string modifiedBy)
	{
		if (string.IsNullOrWhiteSpace(newLogin))
			return Result.Fail("Password hash cannot be empty.");

		Login = newLogin;
		ModifiedOn = DateTime.UtcNow;
		ModifiedBy = modifiedBy;

		return Result.Ok();
	}

	public Result UpdatePassword(string newPasswordHash, string modifiedBy)
	{
		if (string.IsNullOrWhiteSpace(newPasswordHash))
			return Result.Fail("Password hash cannot be empty.");

		PasswordHash = newPasswordHash;
		ModifiedOn = DateTime.UtcNow;
		ModifiedBy = modifiedBy;

		return Result.Ok();
	}

	public void UpdateUser(string name, int gender, DateTime? birthday, Email email, string modifiedBy)
	{
		var validationResult = ValidateUserData(name, gender, birthday, email);
		if (validationResult.IsFailed)
			throw new ArgumentException(string.Join("; ", validationResult.Errors));

		Name = name;
		Gender = gender;
		Birthday = birthday;
		Email = email;
		ModifiedOn = DateTime.UtcNow;
		ModifiedBy = modifiedBy;
	}

	public Result Revoke(string revokedBy)
	{
		RevokedOn = DateTime.UtcNow;
		RevokedBy = revokedBy;
		return Result.Ok();
	}

	public bool VerifyPassword(string password)
	{
		using var sha256 = SHA256.Create();
		var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
		var hashString = Convert.ToBase64String(hashBytes);
		return PasswordHash == hashString;
	}

	public Result Restore(string modifiedBy)
	{
		if (RevokedOn == null)
			return Result.Fail("User is not revoked.");

		RevokedOn = null;
		RevokedBy = null;
		ModifiedOn = DateTime.UtcNow;
		ModifiedBy = modifiedBy;

		return Result.Ok();
	}


}
