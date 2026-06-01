using System.Text.Json;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Projects.Infrastructure.Persistence;
using Users.Application.Interfaces;
using Users.Domain.Common;
using Users.Domain.Entities;
using Users.Domain.Interfaces;
using Users.Domain.ValueObjects;
using Users.Infrastructure.Data;
using Users.Infrastructure.Models;
using Users.Persistence.Common;

namespace Users.IntegrationTests;

public sealed class DiplomaAcceptanceIntegrationTests : IDisposable
{
	private readonly SqliteConnection _connection;
	private readonly UsersDbContext _db;

	public DiplomaAcceptanceIntegrationTests()
	{
		_connection = new SqliteConnection("Data Source=:memory:");
		_connection.Open();

		var options = new DbContextOptionsBuilder<UsersDbContext>()
			.UseSqlite(_connection)
			.Options;

		_db = new UsersDbContext(options);
		_db.Database.EnsureCreated();
	}

	[Fact]
	public async Task Registration_PersistsUserAndOutboxMessage_InOneServiceDatabase()
	{
		var user = User.Register(
			login: "DiplomaUser",
			passwordHash: "hash",
			name: "DiplomaUser",
			gender: 1,
			birthday: new DateTime(2004, 9, 24, 0, 0, 0, DateTimeKind.Utc),
			email: new Email("diploma.user@example.com"),
			createdBy: "self-registration");

		_db.Users.Add(new UserData
		{
			Id = user.Id,
			Login = user.Login,
			PasswordHash = user.PasswordHash,
			Name = user.Name,
			Gender = user.Gender,
			Birthday = user.Birthday,
			Email = user.Email.ToString(),
			CreatedAt = user.CreatedAt,
			CreatedBy = user.CreatedBy,
			Admin = false
		});

		var unitOfWork = new UnitOfWork(_db, new NoOpDomainEventDispatcher(), NullLogger<UnitOfWork>.Instance);
		unitOfWork.TrackEntity(user);

		await unitOfWork.SaveChangesAsync();

		var savedUser = await _db.Users.SingleAsync(x => x.Id == user.Id);
		var outbox = await _db.OutboxMessages.SingleAsync(x => x.AggregateId == user.Id);

		Assert.Equal("DiplomaUser", savedUser.Login);
		Assert.Equal("users.created", outbox.EventType);
		Assert.Equal("users", outbox.Topic);
		Assert.False(outbox.Processed);
	}

	[Fact]
	public async Task IncomingEventStore_SavesIncomingKafkaEvent_AndSkipsDuplicateEventId()
	{
		var store = new IncomingEventStore(_db);
		var eventId = Guid.NewGuid();
		var projectId = Guid.NewGuid();
		var payload = JsonSerializer.Serialize(new
		{
			eventId,
			aggregateId = projectId,
			aggregateType = "Project",
			eventType = "projects.confirm-requested",
			projectId,
			userId = Guid.NewGuid(),
			occurredOnUtc = DateTime.UtcNow
		});

		await store.SaveAsync("user-confirm-requests", projectId.ToString(), payload, CancellationToken.None);
		await store.SaveAsync("user-confirm-requests", projectId.ToString(), payload, CancellationToken.None);

		var events = await _db.IncomingEvents.Where(x => x.EventId == eventId).ToListAsync();

		Assert.Single(events);
		Assert.Equal("projects.confirm-requested", events[0].EventType);
		Assert.False(events[0].Processed);
	}

	public void Dispose()
	{
		_db.Dispose();
		_connection.Dispose();
	}

	private sealed class NoOpDomainEventDispatcher : IDomainEventDispatcher
	{
		public Task DispatchAsync(IEnumerable<IDomainEvent> events, CancellationToken ct = default) => Task.CompletedTask;
	}
}
