using Microsoft.EntityFrameworkCore;
using Projects.Application.DTOs;
using Projects.Persistence.Data;
using Projects.Persistence.Models;
using System.Text.Json;
using Tasks.Application.Events;

namespace Projects.Infrastructure.Processors
{
	public class ConfirmResponseProcessor : IIncomingEventProcessor
	{
		private readonly ProjectsDbContext _db;

		public IReadOnlyCollection<string> SupportedEventTypes { get; } =
			new[] { "confirm.processed" };

		public ConfirmResponseProcessor(ProjectsDbContext db) => _db = db;

		public async Task HandleAsync(IncomingEventDto incoming, CancellationToken ct)
		{
			if (string.IsNullOrWhiteSpace(incoming.Payload))
				throw new InvalidOperationException("Empty payload");

			using var doc = JsonDocument.Parse(incoming.Payload!);
			var root = doc.RootElement;

			Guid objectId;
			DateTime confirmedAt;
			int? registeredObjects = null;
			Guid? confirmedByUserId = null;

			// Получаем objectId / projectId
			if (root.TryGetProperty("objectId", out var objProp) &&
				objProp.ValueKind == JsonValueKind.String &&
				Guid.TryParse(objProp.GetString(), out var parsedObj))
			{
				objectId = parsedObj;
			}
			else if (root.TryGetProperty("projectId", out var projProp) &&
					 projProp.ValueKind == JsonValueKind.String &&
					 Guid.TryParse(projProp.GetString(), out var parsedProj))
			{
				objectId = parsedProj;
			}
			else
			{
				throw new InvalidOperationException("objectId/projectId missing");
			}

			if (!root.TryGetProperty("confirmedAt", out var timeProp))
				throw new InvalidOperationException("confirmedAt missing");

			if (timeProp.ValueKind == JsonValueKind.String &&
				DateTime.TryParse(timeProp.GetString(), null, System.Globalization.DateTimeStyles.AdjustToUniversal | System.Globalization.DateTimeStyles.AssumeUniversal, out var parsedTime))
			{
				confirmedAt = DateTime.SpecifyKind(parsedTime, DateTimeKind.Utc);
			}
			else if (timeProp.ValueKind == JsonValueKind.Number && timeProp.TryGetInt64(out var unixMs))
			{
				confirmedAt = DateTimeOffset.FromUnixTimeMilliseconds(unixMs).UtcDateTime;
			}
			else
			{
				throw new InvalidOperationException("confirmedAt invalid format");
			}

			// registeredObjects (необязательное)
			if (root.TryGetProperty("registeredObjects", out var regProp) &&
				regProp.ValueKind == JsonValueKind.Number && regProp.TryGetInt32(out var regVal))
			{
				registeredObjects = regVal;
			}

			// confirmedByUserId (необязательное)
			if (root.TryGetProperty("confirmedByUserId", out var byProp) &&
				byProp.ValueKind == JsonValueKind.String &&
				Guid.TryParse(byProp.GetString(), out var parsedBy))
			{
				confirmedByUserId = parsedBy;
			}

			await using var tx = await _db.Database.BeginTransactionAsync(ct);

			var projects = _db.Set<ProjectEntity>();
			var existing = await projects.FindAsync(new object?[] { objectId }, ct);

			if (existing == null)
			{
				var incomingEntity = await _db.IncomingEvents.FindAsync(new object?[] { incoming.Id }, ct);
				if (incomingEntity != null)
				{
					incomingEntity.RetryCount += 1;
					incomingEntity.NextAttemptAt = DateTimeOffset.UtcNow.AddSeconds(10); // простой backoff
				}

				await _db.SaveChangesAsync(ct);
				await tx.CommitAsync(ct);
				return; 
			}

			// Обновляем проект
			existing.ConfirmedAt = confirmedAt;
			if (confirmedByUserId.HasValue)
				existing.ConfirmedByUserId = confirmedByUserId.Value;

			existing.ConfirmationInfo = registeredObjects.HasValue
				? $"Confirmed"
				: $"Confirmed at {confirmedAt:O}";

			_db.Update(existing);

			// Помечаем событие как обработанное
			var incomingEnt = await _db.IncomingEvents.FindAsync(new object?[] { incoming.Id }, ct);
			if (incomingEnt != null)
			{
				incomingEnt.Processed = true;
				incomingEnt.ProcessedAt = DateTimeOffset.UtcNow;
			}

			await _db.SaveChangesAsync(ct);
			await tx.CommitAsync(ct);
		}
	}
}
