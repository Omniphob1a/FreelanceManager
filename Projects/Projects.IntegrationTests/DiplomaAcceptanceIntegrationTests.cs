using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Projects.Application.Interfaces;
using Projects.Infrastructure.Persistence;
using Projects.Infrastructure.Processors;
using Projects.Persistence.Data;
using Projects.Persistence.Models;
using Testcontainers.PostgreSql;

public class ApiGatewayRoutingTests
{
	[Fact]
	public void GatewayRoutes_MapPublicApiPaths_ToExpectedMicroserviceClusters()
	{
		var appsettingsPath = FindGatewayAppSettings();
		using var document = JsonDocument.Parse(File.ReadAllText(appsettingsPath));
		var routes = document.RootElement
			.GetProperty("ReverseProxy")
			.GetProperty("Routes");

		AssertRoute(routes, "authRoute", "/api/Auth/{**catch-all}", "usersCluster");
		AssertRoute(routes, "usersRoute", "/api/Users/{**catch-all}", "usersCluster");
		AssertRoute(routes, "projectsRoute", "/api/Projects/{**catch-all}", "projectsCluster");
		AssertRoute(routes, "tasksRoute", "/api/ProjectTasks/{**catch-all}", "tasksCluster");
	}

	private static void AssertRoute(JsonElement routes, string name, string path, string cluster)
	{
		var route = routes.GetProperty(name);
		route.GetProperty("Match").GetProperty("Path").GetString().Should().Be(path);
		route.GetProperty("ClusterId").GetString().Should().Be(cluster);
	}

	private static string FindGatewayAppSettings()
	{
		var directory = new DirectoryInfo(AppContext.BaseDirectory);
		while (directory != null)
		{
			var candidate = Path.Combine(directory.FullName, "Gateway", "Gateway", "appsettings.json");
			if (File.Exists(candidate))
				return candidate;

			directory = directory.Parent;
		}

		throw new FileNotFoundException("Gateway appsettings.json was not found from test output path.");
	}
}

public class IncomingEventsIntegrationTests : IAsyncLifetime
{
	private PostgreSqlContainer? _pgContainer;
	private ServiceProvider? _serviceProvider;

	public async Task InitializeAsync()
	{
		_pgContainer = await TestHelpers.StartPostgresContainerAsync("incoming_events_tests", "test", "test");
		var connectionString =
			$"Host={_pgContainer.Hostname};Port={_pgContainer.GetMappedPublicPort(5432)};Database=incoming_events_tests;Username=test;Password=test;Pooling=false";

		_serviceProvider = TestHelpers.BuildServiceProviderForDb(connectionString);
		await TestHelpers.ApplyMigrationsAsync(_serviceProvider);
	}

	public async Task DisposeAsync()
	{
		if (_serviceProvider != null)
			await _serviceProvider.DisposeAsync();

		if (_pgContainer != null)
		{
			await _pgContainer.StopAsync();
			await _pgContainer.DisposeAsync();
		}
	}

	[Fact]
	public async Task IncomingEventStore_SavesKafkaEvent_AndIgnoresDuplicateEventId()
	{
		using var scope = _serviceProvider!.CreateScope();
		var store = scope.ServiceProvider.GetRequiredService<IIncomingEventStore>();
		var db = scope.ServiceProvider.GetRequiredService<ProjectsDbContext>();
		var eventId = Guid.NewGuid();
		var payload = CreateConfirmPayload(eventId, Guid.NewGuid());

		await store.SaveAsync("confirmations", eventId.ToString(), payload, CancellationToken.None);
		await store.SaveAsync("confirmations", eventId.ToString(), payload, CancellationToken.None);

		var events = await db.IncomingEvents.Where(item => item.EventId == eventId).ToListAsync();
		events.Should().ContainSingle();
		events[0].EventType.Should().Be("confirm.processed");
		events[0].Processed.Should().BeFalse();
	}

	[Fact]
	public async Task ConfirmResponseProcessor_ProcessesIncomingEvent_AndUpdatesProjectState()
	{
		using var scope = _serviceProvider!.CreateScope();
		var db = scope.ServiceProvider.GetRequiredService<ProjectsDbContext>();
		var store = scope.ServiceProvider.GetRequiredService<IIncomingEventStore>();
		var repository = scope.ServiceProvider.GetRequiredService<IIncomingEventRepository>();
		var projectId = Guid.NewGuid();
		var eventId = Guid.NewGuid();

		db.Projects.Add(new ProjectEntity
		{
			Id = projectId,
			Title = "Project awaiting confirmation",
			Description = "Created for incoming event processing test",
			OwnerId = Guid.NewGuid(),
			Category = "development",
			CreatedAt = DateTime.UtcNow,
			Status = 0,
			BudgetMin = 100,
			BudgetMax = 200,
			CurrencyCode = "USD",
			Tags = "tests"
		});
		await db.SaveChangesAsync();

		await store.SaveAsync("confirmations", eventId.ToString(), CreateConfirmPayload(eventId, projectId), CancellationToken.None);
		var incoming = (await repository.GetPendingAsync(10, CancellationToken.None)).Single(item => item.EventId == eventId);

		var processor = new ConfirmResponseProcessor(db);
		await processor.HandleAsync(incoming, CancellationToken.None);

		var project = await db.Projects.FindAsync(projectId);
		project!.ConfirmedAt.Should().NotBeNull();
		project.ConfirmationInfo.Should().Be("Confirmed");

		var processed = await db.IncomingEvents.SingleAsync(item => item.EventId == eventId);
		processed.Processed.Should().BeTrue();
		processed.ProcessedAt.Should().NotBeNull();
	}

	private static string CreateConfirmPayload(Guid eventId, Guid projectId)
	{
		return JsonSerializer.Serialize(new
		{
			eventId,
			aggregateId = projectId,
			aggregateType = "Project",
			eventType = "confirm.processed",
			objectId = projectId,
			confirmedAt = DateTime.UtcNow,
			registeredObjects = 1
		});
	}
}
