using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Projects.Infrastructure.Caching;
using StackExchange.Redis;
using System;
using Tasks.Application.Interfaces;
using Tasks.Application.ProjectTasks.Queries.GetProjectTaskById;
using Tasks.Domain.Interfaces;
using Tasks.Infrastructure.Events;
using Tasks.Infrastructure.Kafka;
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

			var kafkaSection = configuration.GetSection("Kafka");
			var kafkaSettings = kafkaSection.Get<KafkaSettings>() ?? new KafkaSettings();
			services.AddSingleton(kafkaSettings);
			services.Configure<KafkaSettings>(kafkaSection);

			services.AddHostedService<MembersConsumerHostedService>();
			services.AddHostedService<ProjectsConsumerHostedService>();

			services.AddScoped<IAuthorizationService, AuthorizationService>();
			services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
			services.AddDistributedMemoryCache();

			return services;
		}
	}
}
