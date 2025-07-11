// Users.Infrastructure.Auth/JwtTokenGenerator.cs
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Users.Application.Interfaces;
using Users.Domain.ValueObjects;
using Users.Domain.Interfaces.Repositories;

namespace Users.Infrastructure.Auth
{
	public class JwtTokenGenerator : IJwtTokenGenerator
	{
		private readonly JwtSettings _settings;
		private readonly IUserRepository _userRepo;

		public JwtTokenGenerator(
			IOptions<JwtSettings> options,
			IUserRepository userRepo)
		{
			_settings = options.Value;
			_userRepo = userRepo;
		}

		public async Task<string> GenerateToken(
			Guid userId,
			string login,
			IEnumerable<string> roleNames)
		{
			var key = new SymmetricSecurityKey(
				Encoding.UTF8.GetBytes(_settings.SecretKey));
			var creds = new SigningCredentials(
				key, SecurityAlgorithms.HmacSha256);

			var claims = new List<Claim>
			{
				new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
				new Claim(ClaimTypes.Name, login)
			};

			claims.AddRange(roleNames
				.Select(r => new Claim(ClaimTypes.Role, r)));

			var permissions = await _userRepo
				.GetUserPermissions(userId, CancellationToken.None);

			foreach (var perm in permissions)
				claims.Add(new Claim("permission", perm));

			var token = new JwtSecurityToken(
				issuer: _settings.Issuer,
				audience: _settings.Audience,
				claims: claims,
				expires: DateTime.UtcNow.AddMinutes(
					_settings.ExpiryMinutes),
				signingCredentials: creds);

			return new JwtSecurityTokenHandler()
				.WriteToken(token);
		}
	}
}
