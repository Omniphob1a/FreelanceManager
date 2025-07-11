using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Projects.Api;

public static class ApiExtensions
{
	public static IServiceCollection AddApiAuthentication(
		this IServiceCollection services,
		IConfiguration configuration)
	{
		var jwt = configuration.GetSection("JwtSettings");

		services
			.AddAuthentication(options =>
			{
				options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
				options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

			})
			.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
			{
				options.IncludeErrorDetails = true;
				options.RequireHttpsMetadata = true;
				options.SaveToken = true;

				options.TokenValidationParameters = new TokenValidationParameters
				{
					ValidIssuer = jwt["Issuer"],
					ValidAudience = jwt["Audience"],
					IssuerSigningKey = new SymmetricSecurityKey(
											  Encoding.UTF8.GetBytes(jwt["SecretKey"]!)),
					ValidateLifetime = true,
					ValidateIssuer = true,
					ValidateAudience = true,
					ValidateIssuerSigningKey = true,
					RoleClaimType = ClaimTypes.Role
				};
				options.Events = new JwtBearerEvents
				{
					OnAuthenticationFailed = ctx =>
					{
						Console.WriteLine($"[Jwt] AUTH FAILED: {ctx.Exception}");
						return Task.CompletedTask;
					},
					OnTokenValidated = ctx =>
					{
						Console.WriteLine("[Jwt] TOKEN VALIDATED");
						return Task.CompletedTask;
					}
				};
			});

		services.AddAuthorization();

		return services;
	}
}
