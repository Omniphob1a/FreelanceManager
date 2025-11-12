// Projects.Persistence.Common.UnitOfWork.cs
using Microsoft.Extensions.Logging;
using Projects.Application.Interfaces;
using Projects.Domain.Common;
using Projects.Domain.Events;
using Projects.Domain.Interfaces;
using Projects.Persistence.Data;
using Projects.Persistence.Models;
using System.Text.Json;

namespace Projects.Persistence.Common
{
	public class UnitOfWork : IUnitOfWork
	{
		private readonly ProjectsDbContext _dbContext;
		private readonly IDomainEventDispatcher _dispatcher; 
		private readonly ILogger<UnitOfWork> _logger;
		private readonly HashSet<EntityBase> _trackedEntities = new();

		private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
		{
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
			DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
		};

		public UnitOfWork(
			ProjectsDbContext dbContext,
			IDomainEventDispatcher dispatcher,
			ILogger<UnitOfWork> logger)
		{
			_dbContext = dbContext;
			_dispatcher = dispatcher;
			_logger = logger;
		}

		public void TrackEntity(EntityBase entity)
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

				// Convert each domain event to OutboxMessage and add to DbContext
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
						// Решение: не прерываем сохранение всего транзакта из-за одного проблемного события,
						// но логируем и продолжаем — можно менять по требованиям
					}
				}

				// Сохраняем агрегаты + outbox записи в одной транзакции
				var result = await _dbContext.SaveChangesAsync(ct);

				// Очистка domain events и tracked set
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
