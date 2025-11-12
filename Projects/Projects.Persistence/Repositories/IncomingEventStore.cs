using Microsoft.EntityFrameworkCore;
using Projects.Application.Interfaces;
using Projects.Persistence.Data;
using Projects.Persistence.Models.ReadModels;
using System.Text.Json;
using Tasks.Application.Events;

namespace Projects.Infrastructure.Persistence
{
	public class IncomingEventStore : IIncomingEventStore
	{
		private readonly ProjectsDbContext _db;

		public IncomingEventStore(ProjectsDbContext db) => _db = db;

		public async Task SaveAsync(string topic, string? key, string? payload, CancellationToken ct)
		{
			var incoming = new IncomingEvent
			{
				Topic = topic ?? "",
				Key = key,
				Payload = payload ?? string.Empty,
				OccurredAt = DateTime.UtcNow
			};

			if (payload is null)
			{
				incoming.IsTombstone = true;
				if (Guid.TryParse(key, out var agg)) incoming.AggregateId = agg;
				_db.IncomingEvents.Add(incoming);
				await _db.SaveChangesAsync(ct);
				return;
			}

			try
			{
				using var doc = JsonDocument.Parse(payload);
				var root = doc.RootElement;

				if (root.TryGetProperty("eventId", out var evId) && Guid.TryParse(evId.GetString(), out var guidEv))
					incoming.EventId = guidEv;

				if (root.TryGetProperty("aggregateId", out var aggId) && Guid.TryParse(aggId.GetString(), out var guidAgg))
					incoming.AggregateId = guidAgg;

				if (root.TryGetProperty("eventType", out var et)) incoming.EventType = et.GetString() ?? "";
				if (root.TryGetProperty("aggregateType", out var at)) incoming.AggregateType = at.GetString() ?? "";
				if (root.TryGetProperty("occurredOnUtc", out var occ)
					&& DateTime.TryParse(occ.GetString(), out var dt))
				{
					incoming.OccurredAt = DateTime.SpecifyKind(dt, DateTimeKind.Utc);
				}
			}
			catch (JsonException)
			{
				incoming.LastError = "JsonParseError";
			}

			// дефолты, если в JSON их нет
			incoming.AggregateId ??= Guid.Empty;
			incoming.EventId ??= Guid.Empty;

			// идемпотентность по EventId
			if (incoming.EventId != Guid.Empty)
			{
				var exists = await _db.IncomingEvents.AnyAsync(e => e.EventId == incoming.EventId, ct);
				if (exists) return;
			}

			_db.IncomingEvents.Add(incoming);
			await _db.SaveChangesAsync(ct);
		}
	}
}