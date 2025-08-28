using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Polly;
using System.Security.Claims;
using System.Text;
using Tasks.Application.Interfaces;
using Tasks.Infrastructure.Services;

namespace Tasks.Api
{
	public static class ApiExtensions
	{
		public static IServiceCollection AddApi(this IServiceCollection services, IConfiguration configuration)
		{
			services.AddHttpClient<IProjectService, ProjectService>(client =>
			{
				client.BaseAddress = new Uri(configuration["ProjectService:BaseUrl"]); 
				client.Timeout = TimeSpan.FromSeconds(5);
			})
			.AddTransientHttpErrorPolicy(policyBuilder =>
				policyBuilder.WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromMilliseconds(200 * retryAttempt)));

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
}
