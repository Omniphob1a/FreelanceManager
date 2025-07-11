using System.Text;
using Mapster;
using MapsterMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Users.Application.Interfaces;
using Users.Domain.Interfaces.Repositories;
using Users.Infrastructure.Auth;
using Users.Infrastructure.Data;
using Users.Infrastructure.Repositories;

public static class InfrastructureExtensions
{
	public static IServiceCollection AddInfrastructure(
		this IServiceCollection services,
		IConfiguration configuration)
	{

		services.AddDbContext<UsersDbContext>(opts =>
			opts.UseNpgsql(
				configuration.GetConnectionString(nameof(UsersDbContext)),
				npgsql => npgsql.MigrationsAssembly(typeof(UsersDbContext).Assembly.FullName)
			)
		);

		services.AddScoped<IUserRepository, UserRepository>();
		services.AddScoped<IRoleRepository, RoleRepository>();
		services.AddScoped<IPermissionRepository, PermissionRepository>();
		services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
		services.AddSingleton<IAuthorizationHandler, PermissionHandler>();
		services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));
		services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

		var config = new TypeAdapterConfig();

		new UserMappingConfiguration().Register(config);
		new RoleMappingConfiguration().Register(config);

		services.AddSingleton(config);

		services.AddScoped<IMapper>(sp =>
		{
			var cfg = sp.GetRequiredService<TypeAdapterConfig>();
			return new Mapper(cfg);
		});



		services.AddHttpContextAccessor();

		return services;
	}
}
