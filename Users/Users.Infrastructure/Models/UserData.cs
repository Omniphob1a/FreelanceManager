using Users.Domain.ValueObjects;

namespace Users.Infrastructure.Models
{
	public class UserData
	{
		public Guid Id { get; set; }
		public string Login { get; set; }
		public string PasswordHash { get; set; }
		public string Name { get; set; }
		public int Gender { get; set; }
		public DateTime? Birthday { get; set; }
		public bool Admin { get; set; }
		public string Email { get; set; }
		public DateTime CreatedAt { get; set; }
		public string CreatedBy { get; set; }
		public DateTime? ModifiedOn { get; set; }
		public string? ModifiedBy { get; set; }
		public DateTime? RevokedOn { get; set; }
		public string? RevokedBy { get; set; }
		public ICollection<UserRoleEntity> UserRoles { get; set; } = new List<UserRoleEntity>();
	}
}
