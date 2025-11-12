using System;
using System.Collections.Generic;
using System.Linq;
using Users.Domain.Common;
using Users.Domain.Events;
using Users.Domain.ValueObjects;

namespace Users.Domain.Entities
{
	public class User : EntityBase
	{
		public Guid Id { get; }
		public string Login { get; private set; }
		public string PasswordHash { get; private set; }
		public string Name { get; private set; }
		public int Gender { get; private set; }
		public DateTime Birthday { get; private set; }
		public bool Admin { get; private set; }
		public Email Email { get; private set; }
		public DateTime CreatedAt { get; }
		public string CreatedBy { get; }
		public DateTime? ModifiedOn { get; private set; }
		public string? ModifiedBy { get; private set; }
		public DateTime? RevokedOn { get; private set; }
		public string? RevokedBy { get; private set; }

		private readonly List<Guid> _roleIds = new();
		public IReadOnlyCollection<Guid> RoleIds => _roleIds.AsReadOnly();

		private User(
			Guid id,
			string login,
			string passwordHash,
			string name,
			int gender,
			DateTime birthday,
			Email email,
			string createdBy,
			DateTime createdAtUtc)
		{
			if (string.IsNullOrWhiteSpace(login)) throw new ArgumentException("Login is required.", nameof(login));
			if (string.IsNullOrWhiteSpace(passwordHash)) throw new ArgumentException("Password hash is required.", nameof(passwordHash));
			if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name is required.", nameof(name));
			if (email is null) throw new ArgumentNullException(nameof(email));

			Id = id;
			Login = login;
			PasswordHash = passwordHash;
			Name = name;
			Gender = gender;
			Birthday = birthday;
			Email = email;
			CreatedBy = createdBy ?? throw new ArgumentNullException(nameof(createdBy));
			CreatedAt = createdAtUtc;
		}

		// ==========================
		// Factory method
		// ==========================
		public static User Register(
			string login,
			string passwordHash,
			string name,
			int gender,
			DateTime birthday,
			Email email,
			string createdBy)
		{
			var user = new User(
				Guid.NewGuid(),
				login,
				passwordHash,
				name,
				gender,
				birthday,
				email,
				createdBy,
				DateTime.UtcNow);

			user.AddDomainEvent(new UserRegisteredDomainEvent(user.Id, user.Login, user.Name, user.Birthday, user.Gender));
			return user;
		}

		// ==========================
		// Update methods
		// ==========================
		public void UpdateProfile(string name, int gender, DateTime birthday, Email email, string modifiedBy)
		{
			if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name is required.", nameof(name));
			if (email is null) throw new ArgumentNullException(nameof(email));

			Name = name;
			Gender = gender;
			Birthday = birthday;
			Email = email;

			ModifiedBy = modifiedBy;
			ModifiedOn = DateTime.UtcNow;

			AddDomainEvent(new UserProfileUpdatedDomainEvent(Id));
		}

		public void ChangeLogin(string newLogin, string modifiedBy)
		{
			if (string.IsNullOrWhiteSpace(newLogin)) throw new ArgumentException("Login is required.", nameof(newLogin));

			Login = newLogin;
			ModifiedBy = modifiedBy;
			ModifiedOn = DateTime.UtcNow;

			AddDomainEvent(new UserLoginChangedDomainEvent(Id, newLogin));
		}

		public void ChangePassword(string newPasswordHash, string modifiedBy)
		{
			if (string.IsNullOrWhiteSpace(newPasswordHash)) throw new ArgumentException("Password hash is required.", nameof(newPasswordHash));

			PasswordHash = newPasswordHash;
			ModifiedBy = modifiedBy;
			ModifiedOn = DateTime.UtcNow;

			AddDomainEvent(new UserPasswordChangedDomainEvent(Id));
		}

		// ==========================
		// Role management
		// ==========================
		public void AddRole(Guid roleId, string modifiedBy)
		{
			if (roleId == Guid.Empty) throw new ArgumentException("RoleId cannot be empty.", nameof(roleId));
			if (_roleIds.Contains(roleId)) return;

			_roleIds.Add(roleId);
			ModifiedBy = modifiedBy;
			ModifiedOn = DateTime.UtcNow;

			AddDomainEvent(new UserRoleAddedDomainEvent(Id, roleId));
		}

		public void RemoveRole(Guid roleId, string modifiedBy)
		{
			if (!_roleIds.Contains(roleId)) return;

			_roleIds.Remove(roleId);
			ModifiedBy = modifiedBy;
			ModifiedOn = DateTime.UtcNow;

			AddDomainEvent(new UserRoleRemovedDomainEvent(Id, roleId));
		}

		// ==========================
		// Deletion / Restore
		// ==========================
		public void Delete(string revokedBy)
		{
			if (RevokedOn.HasValue)
				throw new InvalidOperationException("User already revoked.");

			RevokedOn = DateTime.UtcNow;
			RevokedBy = revokedBy;

			AddDomainEvent(new UserDeletedDomainEvent(Id));
		}

		public void Restore(string modifiedBy)
		{
			if (!RevokedOn.HasValue)
				throw new InvalidOperationException("User is not revoked.");

			RevokedOn = null;
			RevokedBy = null;
			ModifiedBy = modifiedBy;
			ModifiedOn = DateTime.UtcNow;

			AddDomainEvent(new UserRestoredDomainEvent(Id));
		}

		// ==========================
		// Rehydration (for EF Core)
		// ==========================
		public static User Restore(
			Guid id,
			string login,
			string passwordHash,
			string name,
			int gender,
			DateTime birthday,
			bool admin,
			Email email,
			string createdBy,
			DateTime createdAt,
			DateTime? modifiedOn,
			string? modifiedBy,
			DateTime? revokedOn,
			string? revokedBy,
			IEnumerable<Guid>? roleIds)
		{
			var user = new User(id, login, passwordHash, name, gender, birthday, email, createdBy, createdAt)
			{
				Birthday = birthday,
				Admin = admin,
				ModifiedOn = modifiedOn,
				ModifiedBy = modifiedBy,
				RevokedOn = revokedOn,
				RevokedBy = revokedBy
			};

			if (roleIds != null)
				user._roleIds.AddRange(roleIds);

			return user;
		}
	}
}
