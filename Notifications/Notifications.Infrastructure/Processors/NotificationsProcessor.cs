using Microsoft.EntityFrameworkCore;
using Notifications.Application.DTOs;
using Notifications.Application.Events;
using Notifications.Persistence.Data;
using Notifications.Persistence.Models.Entities;
using Notifications.Domain.Aggregates.Notification.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Notifications.Infrastructure.Processors
{
	public class NotificationsProcessor : IIncomingEventProcessor
	{
		private readonly NotificationsDbContext _db;
		public NotificationsProcessor(NotificationsDbContext db) => _db = db;

		public IReadOnlyCollection<string> SupportedEventTypes => new[] {
			"projects.member.added", "projects.updated","projects.deleted","projects.completed", "projects.published", "projects.archived",
			"tasks.assigned", "tasks.unassigned",
		};

		public async Task HandleAsync(IncomingEventDto incoming, CancellationToken ct)
		{
			if (string.IsNullOrWhiteSpace(incoming.Payload))
				throw new InvalidOperationException("Empty payload");

			using var doc = JsonDocument.Parse(incoming.Payload!);
			var root = doc.RootElement;

			if (!(root.TryGetProperty("aggregateId", out var aggProp)
				  && aggProp.ValueKind == JsonValueKind.String
				  && Guid.TryParse(aggProp.GetString(), out var aggregateId)))
				throw new InvalidOperationException("aggregateId missing");

			if (!(root.TryGetProperty("eventId", out var evProp)
				  && evProp.ValueKind == JsonValueKind.String
				  && Guid.TryParse(evProp.GetString(), out var eventId)))
				throw new InvalidOperationException("eventId missing");

			// detect tombstone / delete
			bool isDelete = incoming.IsTombstone;
			if (root.TryGetProperty("isTombstone", out var tomb) && tomb.ValueKind == JsonValueKind.True) isDelete = true;
			if (root.TryGetProperty("eventType", out var evt) && evt.ValueKind == JsonValueKind.String &&
				evt.GetString()!.EndsWith("deleted", StringComparison.OrdinalIgnoreCase)) isDelete = true;

			// Start a DB transaction to create notification and mark incoming as processed atomically
			await using var tx = await _db.Database.BeginTransactionAsync(ct);
			try
			{
				// Load the IncomingEvent entity to update processed flags later
				var incomingEntity = await _db.IncomingEvents.FirstOrDefaultAsync(x => x.Id == incoming.Id, ct);
				if (incomingEntity == null)
				{
					// Strange state: nothing to update — treat as processed (avoid infinite loop)
					await tx.CommitAsync(ct);
					return;
				}

				// Decide handling by event type
				var eventType = incoming.EventType?.ToLowerInvariant() ?? string.Empty;

				// We'll build a list of (targetUserId, templateKey) for which to create notifications
				var toCreate = new List<(Guid userId, string templateKey)>();

				switch (eventType)
				{
					case "tasks.assigned":
						{
							// Prefer explicit assigneeId in payload
							Guid? assigneeId = TryReadGuid(root, "assigneeId")
								?? TryReadGuid(root, "assignee")   // some payloads use different property
								?? null;

							// If not present, we cannot resolve here — mark processed to avoid retry or throw to retry later.
							// Decision: if no assignee in payload, skip creation (mark processed) — adjust to your needs.
							if (!assigneeId.HasValue)
							{
								// no target to notify
								incomingEntity.Processed = true;
								incomingEntity.ProcessedAt = DateTimeOffset.UtcNow;
								_db.IncomingEvents.Update(incomingEntity);
								await _db.SaveChangesAsync(ct);
								await tx.CommitAsync(ct);
								return;
							}

							toCreate.Add((assigneeId.Value, "task.assigned.v1"));
							break;
						}

					case "projects.member.added":
						{
							// Prefer explicit memberId/userId in payload
							Guid? memberId = TryReadGuid(root, "memberId")
								?? TryReadGuid(root, "userId")
								?? null;

							if (!memberId.HasValue)
							{
								// fallback: maybe aggregateId is projectId and payload contains 'addedUserId' etc.
								// For MVP — if we can't find user, mark processed and skip.
								incomingEntity.Processed = true;
								incomingEntity.ProcessedAt = DateTimeOffset.UtcNow;
								_db.IncomingEvents.Update(incomingEntity);
								await _db.SaveChangesAsync(ct);
								await tx.CommitAsync(ct);
								return;
							}

							toCreate.Add((memberId.Value, "project.member.added.v1"));
							break;
						}

					// add other event types here if you want minimal handling
					default:
						{
							// Unsupported event type for this processor — mark processed (or choose to throw to retry)
							incomingEntity.Processed = true;
							incomingEntity.ProcessedAt = DateTimeOffset.UtcNow;
							_db.IncomingEvents.Update(incomingEntity);
							await _db.SaveChangesAsync(ct);
							await tx.CommitAsync(ct);
							return;
						}
				}

				// For each resolved target create Notification + Delivery (idempotently)
				foreach (var (userId, templateKey) in toCreate)
				{
					// idempotency: check if notification for this EventId + UserId already exists
					var exists = await _db.Notifications
						.AsNoTracking()
						.FirstOrDefaultAsync(n => n.EventId == eventId && n.UserId == userId, ct);

					if (exists != null)
						continue; // already created

					// Create Notification entity
					var nEnt = new NotificationEntity
					{
						Id = Guid.NewGuid(),
						EventId = eventId,
						UserId = userId,
						TemplateKey = templateKey,
						PayloadRaw = incoming.Payload,
						PayloadRendered = null, // optionally render here
						CreatedAt = DateTimeOffset.UtcNow
					};

					// Default channel: InApp (you can extend Channels via payload/routing)
					var delivery = new NotificationDeliveryEntity
					{
						Id = Guid.NewGuid(),
						NotificationId = nEnt.Id,
						Channel = (int)NotificationChannel.InApp,
						Status = (int)DeliveryStatus.Pending,
						Attempts = 0,
						CreatedAt = DateTimeOffset.UtcNow
					};

					nEnt.Deliveries.Add(delivery);
					_db.Notifications.Add(nEnt);
				}

				// mark incoming processed and save everything in one transaction
				incomingEntity.Processed = true;
				incomingEntity.ProcessedAt = DateTimeOffset.UtcNow;
				_db.IncomingEvents.Update(incomingEntity);

				await _db.SaveChangesAsync(ct);
				await tx.CommitAsync(ct);
			}
			catch
			{
				await tx.RollbackAsync(ct);
				// rethrow so outer hosted-service will IncrementRetry / schedule next attempt
				throw;
			}
		}

		private static Guid? TryReadGuid(JsonElement root, string propName)
		{
			if (!root.TryGetProperty(propName, out var prop)) return null;
			if (prop.ValueKind != JsonValueKind.String) return null;
			if (Guid.TryParse(prop.GetString(), out var g)) return g;
			return null;
		}
	}
}
