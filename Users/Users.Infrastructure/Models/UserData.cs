using Users.Domain.ValueObjects;

namespace Users.Infrastructure.Models
{
	public class UserData
	{
		public Guid Id { get; set; }
		public string Login { get; set; } = default!;
		public string PasswordHash { get; set; } = default!;
		public string Name { get; set; } = default!;
		public int Gender { get; set; }
		public DateTime Birthday { get; set; } = default!;
		public bool Admin { get; set; }
		public string Email { get; set; } = default!;
		public DateTime CreatedAt { get; set; }
		public string CreatedBy { get; set; } = default!;
		public DateTime? ModifiedOn { get; set; } = default!;
		public string? ModifiedBy { get; set; } = default!;
		public DateTime? RevokedOn { get; set; } = default!;
		public string? RevokedBy { get; set; } = default!;
		public ICollection<UserRoleEntity> UserRoles { get; set; } = new List<UserRoleEntity>();
	}
}
