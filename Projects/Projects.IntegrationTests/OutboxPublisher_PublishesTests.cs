using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Projects.Infrastructure.Kafka;
using Projects.Persistence.Data;
using System;
using System.Threading;
using System.Threading.Tasks;
using Testcontainers.PostgreSql;
using Xunit;
using Xunit.Abstractions;

public class OutboxPublisher_PublishesTests : IAsyncLifetime
{
	private PostgreSqlContainer? _pgContainer;
	private string _connectionString = string.Empty;
	private readonly ITestOutputHelper _output;

	public OutboxPublisher_PublishesTests(ITestOutputHelper output)
	{
		_output = output;
	}

	public async Task InitializeAsync()
	{
		_output.WriteLine("=== Starting Postgres container ===");
		_pgContainer = await TestHelpers.StartPostgresContainerAsync("testdb2", "test", "test");
		var host = _pgContainer.Hostname;
		var hostPort = _pgContainer.GetMappedPublicPort(5432);
		_connectionString = $"Host={host};Port={hostPort};Database=testdb2;Username=test;Password=test;Pooling=false";
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
	public async Task OutboxPublisher_PublishesAndMarksProcessed_WithLogs()
	{
		_output.WriteLine("=== Arrange: creating mocks and service provider ===");
		Mock<IKafkaProducer> producerMock = new Mock<IKafkaProducer>(MockBehavior.Strict);
		producerMock
			.Setup(p => p.ProduceAsync(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
			.Returns(Task.CompletedTask)
			.Verifiable();

		var sp = TestHelpers.BuildServiceProviderForDb(_connectionString, services =>
		{
			services.AddSingleton<IKafkaProducer>(producerMock.Object);
			services.AddSingleton<Mock<IKafkaProducer>>(producerMock);
		});

		_output.WriteLine("Applying migrations...");
		await TestHelpers.ApplyMigrationsAsync(sp);
		_output.WriteLine("Migrations applied successfully.");

		// Seed outbox
		using (var scope = sp.CreateScope())
		{
			var db = scope.ServiceProvider.GetRequiredService<ProjectsDbContext>();

			var outbox = new Projects.Persistence.Models.OutboxMessage
			{
				EventId = Guid.NewGuid(),
				AggregateId = Guid.NewGuid(),
				AggregateType = "Project",
				EventType = "projects.created",
				Version = 1,
				Topic = "projects",
				Key = Guid.NewGuid().ToString(),
				Payload = "{\"dummy\":true}",
				OccurredAt = DateTime.UtcNow,
				Processed = false,
				NextAttemptAt = DateTimeOffset.UtcNow
			};

			db.OutboxMessages.Add(outbox);
			await db.SaveChangesAsync();
			_output.WriteLine($"Outbox message seeded: EventId={outbox.EventId}");
		}

		_output.WriteLine("Starting hosted service...");
		var logger = new NullLogger<Projects.Infrastructure.Outbox.OutboxPublisherHostedService>();
		var hosted = new Projects.Infrastructure.Outbox.OutboxPublisherHostedService(sp, sp.GetRequiredService<IKafkaProducer>(), logger);

		using var cts = new CancellationTokenSource();
		var startTask = hosted.StartAsync(cts.Token);

		var sw = System.Diagnostics.Stopwatch.StartNew();
		bool processed = false;
		while (sw.Elapsed < TimeSpan.FromSeconds(8))
		{
			using var scope = sp.CreateScope();
			var db = scope.ServiceProvider.GetRequiredService<ProjectsDbContext>();
			var fresh = await db.OutboxMessages.FirstOrDefaultAsync(o => !o.Processed);
			if (fresh == null)
			{
				processed = true;
				break;
			}
			await Task.Delay(200);
		}

		cts.Cancel();
		await hosted.StopAsync(CancellationToken.None);
		_output.WriteLine("Hosted service stopped.");

		var producerMockResolved = sp.GetRequiredService<Mock<IKafkaProducer>>();
		producerMockResolved.Verify(p => p.ProduceAsync(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);

		processed.Should().BeTrue();
		_output.WriteLine("✅ Test passed: Outbox message was processed and producer called.");
	}
}
