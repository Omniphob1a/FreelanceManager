using System.Text.Json;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Tasks.Application.DTOs;
using Tasks.Application.Interfaces;
using Tasks.Domain.Aggregate.Enums.Tasks.Domain.Aggregate.Enums;
using Tasks.Domain.Aggregate.Root;
using Tasks.Domain.Common;
using Tasks.Domain.Interfaces;
using Tasks.Infrastructure.Persistence;
using Tasks.Infrastructure.Processors;
using Tasks.Persistence.Common;
using Tasks.Persistence.Data;
using Tasks.Persistence.Mappings;
using Tasks.Persistence.Models.ReadModels;

namespace Tasks.IntegrationTests;

public sealed class DiplomaAcceptanceIntegrationTests : IDisposable
{
	private readonly SqliteConnection _connection;
	private readonly ProjectTasksDbContext _db;

	public DiplomaAcceptanceIntegrationTests()
	{
		_connection = new SqliteConnection("Data Source=:memory:");
		_connection.Open();

		var options = new DbContextOptionsBuilder<ProjectTasksDbContext>()
			.UseSqlite(_connection)
			.Options;

		_db = new ProjectTasksDbContext(options);
		_db.Database.EnsureCreated();
	}

	[Fact]
	public async Task CreateTask_PersistsTaskAndOutboxMessage_InOneServiceDatabase()
	{
		var mapper = new ProjectTaskMapper(
			NullLogger<ProjectTaskMapper>.Instance,
			new TimeEntryMapper(),
			new CommentMapper());
		var task = ProjectTask.CreateDraft(
			projectId: Guid.NewGuid(),
			title: "Diploma task",
			description: "Created by integration test",
			reporterId: Guid.NewGuid(),
			assigneeId: Guid.NewGuid(),
			priority: TaskPriority.High);

		_db.Tasks.Add(mapper.ToEntity(task));

		var unitOfWork = new UnitOfWork(_db, new NoOpDomainEventDispatcher(), NullLogger<UnitOfWork>.Instance);
		unitOfWork.TrackEntity(task);

		await unitOfWork.SaveChangesAsync();

		var savedTask = await _db.Tasks.SingleAsync(x => x.Id == task.Id);
		var outbox = await _db.OutboxMessages.SingleAsync(x => x.AggregateId == task.Id);

		Assert.Equal("Diploma task", savedTask.Title);
		Assert.Equal("tasks.created", outbox.EventType);
		Assert.Equal("tasks", outbox.Topic);
		Assert.False(outbox.Processed);
	}

	[Fact]
	public async Task IncomingProjectEvent_IsStoredProcessedAndAppliedIdempotently()
	{
		var store = new IncomingEventStore(_db);
		var eventId = Guid.NewGuid();
		var projectId = Guid.NewGuid();
		var ownerId = Guid.NewGuid();
		var payload = JsonSerializer.Serialize(new
		{
			eventId,
			aggregateId = projectId,
			aggregateType = "Project",
			eventType = "projects.created",
			title = "Kafka project",
			ownerId,
			occurredOnUtc = DateTime.UtcNow
		});

		await store.SaveAsync("projects", projectId.ToString(), payload, CancellationToken.None);
		await store.SaveAsync("projects", projectId.ToString(), payload, CancellationToken.None);

		var incomingEvents = await _db.IncomingEvents.Where(x => x.EventId == eventId).ToListAsync();
		Assert.Single(incomingEvents);

		var processor = new ProjectEventsProcessor(_db);
		var dto = ToDto(incomingEvents[0]);
		await processor.HandleAsync(dto, CancellationToken.None);
		await processor.HandleAsync(dto, CancellationToken.None);

		var projects = await _db.Set<ProjectReadModel>().Where(x => x.Id == projectId).ToListAsync();
		var processed = await _db.IncomingEvents.SingleAsync(x => x.EventId == eventId);

		Assert.Single(projects);
		Assert.Equal("Kafka project", projects[0].Title);
		Assert.True(processed.Processed);
		Assert.NotNull(processed.ProcessedAt);
	}

	public void Dispose()
	{
		_db.Dispose();
		_connection.Dispose();
	}

	private static IncomingEventDto ToDto(IncomingEvent incoming) => new()
	{
		Id = incoming.Id,
		EventId = incoming.EventId,
		AggregateId = incoming.AggregateId,
		AggregateType = incoming.AggregateType,
		EventType = incoming.EventType,
		Payload = incoming.Payload,
		OccurredAt = incoming.OccurredAt,
		IsTombstone = incoming.IsTombstone,
		RetryCount = incoming.RetryCount,
		NextAttemptAt = incoming.NextAttemptAt
	};

	private sealed class NoOpDomainEventDispatcher : IDomainEventDispatcher
	{
		public Task DispatchAsync(IEnumerable<IDomainEvent> events, CancellationToken ct = default) => Task.CompletedTask;
	}
}
