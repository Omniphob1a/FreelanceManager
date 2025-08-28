using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Projects.Infrastructure.Caching;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tasks.Application.Interfaces;
using Tasks.Application.ProjectTasks.Queries.GetProjectTaskById;
using Tasks.Domain.Interfaces;
using Tasks.Infrastructure.Events;
using Tasks.Infrastructure.Services;

namespace Tasks.Infrastructure
{
	public static class InfrastructureExtensions
	{
		public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
		{
			services.AddStackExchangeRedisCache(opt =>
				opt.Configuration = configuration.GetConnectionString("Redis"));

			services.AddScoped<ICacheService, RedisCacheService>();
			services.AddSingleton<IConnectionMultiplexer>(sp =>
				ConnectionMultiplexer.Connect(configuration.GetConnectionString("Redis")));

			services.AddMediatR(cfg =>
			{
				cfg.RegisterServicesFromAssemblies(typeof(GetProjectTaskByIdQuery).Assembly);
			});
			services.AddScoped<ICurrentUserService, CurrentUserService>();
			services.AddHttpClient<IProjectService, ProjectService>(client =>
			{
				client.BaseAddress = new Uri("http://gateway:8080/api/");
			});
			services.AddScoped<IAuthorizationService, AuthorizationService>();
			services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
			services.AddDistributedMemoryCache();
			return services;
		}
	}
}
