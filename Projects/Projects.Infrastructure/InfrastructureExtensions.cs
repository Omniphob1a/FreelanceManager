using Amazon;
using Amazon.S3;
using Hangfire;
using Hangfire.PostgreSql;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Projects.Application.Common.Abstractions;
using Projects.Application.Common.Behaviors;
using Projects.Application.Interfaces;
using Projects.Application.Outbox;
using Projects.Application.Projects.Queries.GetProjectById;
using Projects.Infrastructure.Caching;
using Projects.Infrastructure.Events;
using Projects.Infrastructure.FileStorage;
using Projects.Infrastructure.Hangfire;
using Projects.Infrastructure.Kafka;
using Projects.Infrastructure.Options;
using Projects.Infrastructure.Outbox;
using Projects.Infrastructure.Services;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Tasks.Infrastructure.Services;

namespace Projects.Infrastructure
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
				cfg.RegisterServicesFromAssemblies(typeof(GetProjectByIdQuery).Assembly);
			});

			services.Configure<S3Options>(configuration.GetSection("S3"));

			services.AddSingleton<IAmazonS3>(sp =>
			{
				var options = sp.GetRequiredService<IOptions<S3Options>>().Value;

				var config = new AmazonS3Config
				{
					ServiceURL = options.ServiceUrl,
					ForcePathStyle = true
				};

				return new AmazonS3Client(options.AccessKey, options.SecretKey, config);
			});
			services.AddHangfire(config =>
			{
				var connectionString = configuration.GetConnectionString("Hangfire")
					?? configuration["Hangfire:Storage:ConnectionString"];

				config.UsePostgreSqlStorage(connectionString, new PostgreSqlStorageOptions
				{
					PrepareSchemaIfNecessary = true,
					QueuePollInterval = TimeSpan.FromSeconds(5)
				});
			});
			services.AddHttpClient<IUserService, UserService>(client =>
			{
				client.BaseAddress = new Uri("http://gateway:8080/api/");
			});
			services.AddScoped<IAuthorizationService, AuthorizationService>();
			services.AddScoped<ICurrentUserService, CurrentUserService>();
			services.AddHangfireServer();
			services.AddScoped<IBackgroundJobManager, BackgroundJobManager>();
			services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
			services.AddScoped<IFileStorage, S3FileStorage>();
			services.AddSingleton(new KafkaSettings { BootstrapServers = "localhost:9092" });
			services.AddSingleton<IKafkaProducer, ConfluentKafkaProducer>();
			services.AddScoped<IOutboxService, OutboxService>();
			services.AddDistributedMemoryCache();
			return services;
		}
	}
}
