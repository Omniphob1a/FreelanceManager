using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Tasks.Application.DTOs;
using Tasks.Application.Events;
using Tasks.Persistence.Data;
using Tasks.Persistence.Models.ReadModels;

namespace Tasks.Infrastructure.Processors
{
	public class ProjectEventsProcessor : IIncomingEventProcessor
	{
		private readonly ProjectTasksDbContext _db;

		public IReadOnlyCollection<string> SupportedEventTypes { get; } =
			new[] { "projects.created", "projects.updated", "projects.removed" };

		public ProjectEventsProcessor(ProjectTasksDbContext db) => _db = db;

		public async Task HandleAsync(IncomingEventDto incoming, CancellationToken ct)
		{
			if (string.IsNullOrWhiteSpace(incoming.Payload))
				throw new InvalidOperationException("Empty payload");

			using var doc = JsonDocument.Parse(incoming.Payload!);
			var root = doc.RootElement;

			// 🔥 AggregateId нужно получить обязательно
			Guid aggregateId;

			if (root.TryGetProperty("aggregateId", out var aggProp) &&
				aggProp.ValueKind == JsonValueKind.String &&
				Guid.TryParse(aggProp.GetString(), out var parsedAgg))
			{
				aggregateId = parsedAgg;
			}
			else if (root.TryGetProperty("projectId", out var projProp) &&
					 projProp.ValueKind == JsonValueKind.String &&
					 Guid.TryParse(projProp.GetString(), out var parsedProj))
			{
				aggregateId = parsedProj;
			}
			else
			{
				throw new InvalidOperationException("AggregateId missing");
			}

			// 🔥 Флаги удаления
			bool isDelete = false;

			if (incoming.IsTombstone)
				isDelete = true;

			if (root.TryGetProperty("isTombstone", out var tomb) &&
				tomb.ValueKind == JsonValueKind.True)
			{
				isDelete = true;
			}

			if (root.TryGetProperty("eventType", out var evt) &&
				evt.ValueKind == JsonValueKind.String &&
				evt.GetString()!.EndsWith("deleted", StringComparison.OrdinalIgnoreCase))
			{
				isDelete = true;
			}

			await using var tx = await _db.Database.BeginTransactionAsync(ct);

			var projects = _db.Set<ProjectReadModel>();
			var existing = await projects.FindAsync(new object?[] { aggregateId }, ct);

			if (isDelete)
			{
				if (existing != null)
					projects.Remove(existing);
			}
			else
			{
				if (existing == null)
				{
					string title = "";
					Guid ownerId = Guid.Empty;

					if (root.TryGetProperty("title", out var titleProp) &&
						titleProp.ValueKind == JsonValueKind.String)
						title = titleProp.GetString() ?? "";

					if (root.TryGetProperty("ownerId", out var ownerProp) &&
						ownerProp.ValueKind == JsonValueKind.String &&
						Guid.TryParse(ownerProp.GetString(), out var parsedOwner))
						ownerId = parsedOwner;

					await projects.AddAsync(new ProjectReadModel
					{
						Id = aggregateId,
						Title = title,
						OwnerId = ownerId
					}, ct);
				}
				else
				{
					bool updated = false;

					if (root.TryGetProperty("title", out var titleProp))
					{
						if (titleProp.ValueKind == JsonValueKind.String)
						{
							var newTitle = titleProp.GetString() ?? "";
							if (!string.Equals(existing.Title, newTitle, StringComparison.Ordinal))
							{
								existing.Title = newTitle;
								updated = true;
							}
						}
					}

					if (root.TryGetProperty("ownerId", out var ownerProp))
					{
						if (ownerProp.ValueKind == JsonValueKind.String &&
							Guid.TryParse(ownerProp.GetString(), out var newOwner))
						{
							if (existing.OwnerId != newOwner)
							{
								existing.OwnerId = newOwner;
								updated = true;
							}
						}
					}

					if (updated)
						_db.Update(existing);
				}
			}

			var incomingEntity = await _db.IncomingEvents.FindAsync(new object?[] { incoming.Id }, ct);
			if (incomingEntity != null)
			{
				incomingEntity.Processed = true;
				incomingEntity.ProcessedAt = DateTimeOffset.UtcNow;
			}

			await _db.SaveChangesAsync(ct);
			await tx.CommitAsync(ct);
		}
	}
}
