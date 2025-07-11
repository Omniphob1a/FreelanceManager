namespace Users.Application.Responses
{
	public class AuthenticationResult
	{
		public Guid UserId { get; init; }
		public string Token { get; init; } = string.Empty;
		public DateTime ExpiresAt { get; init; }
	}
}
