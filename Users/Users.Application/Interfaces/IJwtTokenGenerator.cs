namespace Users.Application.Interfaces
{
	public interface IJwtTokenGenerator
	{
		Task<string> GenerateToken(
			Guid userId,
			string login,
			IEnumerable<string> roleNames);
	}
}
