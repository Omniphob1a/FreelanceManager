using FluentAssertions;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Projects.Application.Interfaces;
using Projects.Application.Outbox;
using Projects.Application.Projects.Commands.CreateProject;
using Projects.Application.Services;
using Projects.Domain.Repositories;
using Projects.Persistence.Data;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Testcontainers.PostgreSql;
using Xunit;
using Xunit.Abstractions;

public class CreateProject_OutboxTests : IAsyncLifetime
{
	private PostgreSqlContainer? _pgContainer;
	private string _connectionString = string.Empty;
	private readonly ITestOutputHelper _output;

	public CreateProject_OutboxTests(ITestOutputHelper output)
	{
		_output = output;
	}

	public async Task InitializeAsync()
	{
		_output.WriteLine("=== Starting Postgres container ===");
		_pgContainer = await TestHelpers.StartPostgresContainerAsync("testdb", "test", "test");
		var host = _pgContainer.Hostname;
		var hostPort = _pgContainer.GetMappedPublicPort(5432);
		_connectionString = $"Host={host};Port={hostPort};Database=testdb;Username=test;Password=test;Pooling=false";
		_output.WriteLine($"Container running at: {_connectionString}");
	}

	public async Task DisposeAsync()
	{
		if (_pgContainer != null)
		{
			try
			{
				await _pgContainer.StopAsync();
				_output.WriteLine("Postgres container stopped.");
			}
			finally
			{
				await _pgContainer.DisposeAsync();
			}
		}
	}

	[Fact]
	public async Task CreateProject_WritesOutbox_WithLogs()
	{
		_output.WriteLine("=== Arrange: build service provider and apply migrations ===");
		var sp = TestHelpers.BuildServiceProviderForDb(_connectionString);
		await TestHelpers.ApplyMigrationsAsync(sp);
		_output.WriteLine("Migrations applied successfully.");

		using (var scope = sp.CreateScope())
		{
			_output.WriteLine("Creating CreateProjectCommandHandler...");
			var handler = new CreateProjectCommandHandler(
				scope.ServiceProvider.GetRequiredService<IProjectRepository>(),
				scope.ServiceProvider.GetRequiredService<IUnitOfWork>(),
				scope.ServiceProvider.GetRequiredService<ILogger<CreateProjectCommandHandler>>(),
				scope.ServiceProvider.GetRequiredService<TagParserService>()
			);

			var tags = new List<string> { "tag1", "tag2" };
			var cmd = new CreateProjectCommand(
				Title: "T1",
				Description: "desc",
				OwnerId: Guid.NewGuid(),
				BudgetMin: 100,
				BudgetMax: 200,
				CurrencyCode: "USD",
				Category: "development",
				Tags: tags
			);

			_output.WriteLine("=== Act: handling CreateProjectCommand ===");
			var res = await handler.Handle(cmd, CancellationToken.None);

			_output.WriteLine("=== Assert: checking results in DB ===");
			res.IsSuccess.Should().BeTrue();
			var projectId = res.Value;
			_output.WriteLine($"Project created with ID: {projectId}");

			var db = scope.ServiceProvider.GetRequiredService<ProjectsDbContext>();
			var projectEntity = await db.Projects.FindAsync(projectId);
			projectEntity.Should().NotBeNull();
			_output.WriteLine("Project entity found in DB.");

			var outbox = await db.OutboxMessages.FirstOrDefaultAsync(o => o.AggregateId == projectId);
			outbox.Should().NotBeNull();
			_output.WriteLine($"Outbox message created: EventType={outbox!.EventType}, Topic={outbox.Topic}");
			outbox!.Processed.Should().BeFalse();
			outbox.EventType.Should().NotBeNullOrEmpty();
			outbox.Topic.Should().Be("projects");
			outbox.Payload.Should().NotBeNull();
			_output.WriteLine("✅ Outbox message validated successfully.");
		}
	}
}
