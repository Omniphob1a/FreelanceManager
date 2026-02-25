using Microsoft.Extensions.Logging;
using System.Text.Json;
using Notifications.Domain.Common;
using Notifications.Domain.Interfaces;
using Notifications.Persistence.Data;
using Notifications.Persistence.Models;
using Notifications.Application.Interfaces;
using Notifications.Persistence.Models.Entities;
using Tasks.Domain.Interfaces;

namespace Notifications.Persistence.Common
{
	public class UnitOfWork : IUnitOfWork
	{
		private readonly NotificationsDbContext _dbContext;
		private readonly ILogger<UnitOfWork> _logger;
		private readonly HashSet<AggregateRoot> _trackedEntities = new();

		private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
		{
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
			DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
		};

		public UnitOfWork(
			NotificationsDbContext dbContext,
			ILogger<UnitOfWork> logger)
		{
			_dbContext = dbContext;
			_logger = logger;
		}

		public void TrackEntity(AggregateRoot entity)
		{
			if (!_trackedEntities.Contains(entity))
				_trackedEntities.Add(entity);
		}

		public async Task<int> SaveChangesAsync(CancellationToken ct = default)
		{
			try
			{
				var eventsToPersist = _trackedEntities
					.SelectMany(e => e.DomainEvents)
					.ToList();

				_logger.LogInformation("Collected {Count} domain events from tracked entities", eventsToPersist.Count);

				foreach (var ev in eventsToPersist)
				{
					try
					{
						string? payload = ev.IsTombstone ? null : JsonSerializer.Serialize(ev, ev.GetType(), _jsonOptions);


						var outbox = new OutboxMessage
						{
							EventId = ev.EventId,
							AggregateId = ev.AggregateId,
							AggregateType = ev.AggregateType,
							EventType = ev.EventType,
							Version = ev.Version,
							Payload = payload,
							OccurredAt = ev.OccurredOnUtc,
							NextAttemptAt = DateTimeOffset.UtcNow,
							Topic = ev.KafkaTopic,
							Key = ev.KafkaKey,
						};

						_dbContext.OutboxMessages.Add(outbox);
					}
					catch (Exception ex)
					{
						_logger.LogError(ex, "Failed to serialize domain event {EventType} for outbox. EventId={EventId}", ev.EventType, ev.EventId);

					}
				}

				var result = await _dbContext.SaveChangesAsync(ct);

				foreach (var agg in _trackedEntities)
					agg.ClearDomainEvents();

				_trackedEntities.Clear();

				_logger.LogInformation("SaveChangesAsync completed. Persisted entities and outbox messages.");

				return result;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "UnitOfWork.SaveChangesAsync failed");
				throw;
			}
		}
	}
}
