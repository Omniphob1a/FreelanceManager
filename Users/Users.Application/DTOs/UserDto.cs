namespace Users.Application.DTOs
{
	public class UserDto
	{
		public Guid Id { get; init; }
		public string Login { get; init; } = "";
		public string Name { get; init; } = "";
		public int Gender { get; init; }
		public DateTime? Birthday { get; init; }
		public string Email { get; init; } = "";
		public bool Admin { get; init; }
		public DateTime CreatedAt { get; init; }
		public DateTime? ModifiedOn { get; init; }
		public DateTime? RevokedOn { get; init; }
		public IEnumerable<string> Roles { get; init; } = Array.Empty<string>();
		public IEnumerable<string> Permissions { get; init; } = Array.Empty<string>();
	}

}
