// TestHelpers.cs
using Mapster;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Projects.Application.Interfaces;
using Projects.Application.Mappings;
using Projects.Application.Services;
using Projects.Domain.Interfaces;
using Projects.Domain.Repositories;
using Projects.Persistence.Common;
using Projects.Persistence.Data;
using Projects.Persistence.Repositories;
using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Testcontainers.PostgreSql;
using Projects.Persistence;
using Projects.Application;
using Projects.Infrastructure;

public static class TestHelpers
{
	public static async Task<PostgreSqlContainer> StartPostgresContainerAsync(string dbName, string user, string password)
	{

		var builder = new PostgreSqlBuilder()
			.WithDatabase(dbName)
			.WithUsername(user)
			.WithPassword(password)
			.WithImage("postgres:15"); 

		var container = builder.Build();
		await container.StartAsync();
		return container;
	}

	public static ServiceProvider BuildServiceProviderForDb(string connectionString,
		Action<IServiceCollection>? customize = null,
		Action<IServiceCollection>? testOverrides = null)
	{
		// 1) Build a minimal IConfiguration containing connection string(s)
		var cfgDict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
		{
			["ConnectionStrings:ProjectsDbContext"] = connectionString
			
		};
		var configuration = new ConfigurationBuilder()
			.AddInMemoryCollection(cfgDict)
			.Build();

		var services = new ServiceCollection(); 
		services.AddSingleton<IConfiguration>(configuration);
		services.AddLogging();

		services.AddApplication(); //
		services.AddInfrastructure(configuration);
		services.AddPersistence(configuration); 

		customize?.Invoke(services);

		testOverrides?.Invoke(services);

		var sp = services.BuildServiceProvider();

		return sp;
	}

	public static async Task ApplyMigrationsAsync(IServiceProvider sp)
	{
		using var scope = sp.CreateScope();
		var db = scope.ServiceProvider.GetRequiredService<ProjectsDbContext>();
		await db.Database.MigrateAsync();
	}

	private class NoOpDomainEventDispatcher : IDomainEventDispatcher
	{
		public Task DispatchAsync(IEnumerable<IDomainEvent> events, CancellationToken ct = default) => Task.CompletedTask;
	}
}
