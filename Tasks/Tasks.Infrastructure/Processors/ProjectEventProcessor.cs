using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Tasks.Application.DTOs;
using Tasks.Application.Events;
using Tasks.Persistence.Data;
using Tasks.Persistence.Models.ReadModels;

namespace Tasks.Infrastructure.Processors
{
	public class ProjectEventsProcessor : IIncomingEventProcessor
	{
		public IReadOnlyCollection<string> SupportedEventTypes { get; } =
			new[] { "projects.created", "projects.updated", "projects.deleted" };

		private readonly ProjectTasksDbContext _db;

		public ProjectEventsProcessor(ProjectTasksDbContext db) => _db = db;

		public async Task HandleAsync(IncomingEventDto incoming, CancellationToken ct)
		{
			if (string.IsNullOrWhiteSpace(incoming.Payload))
				throw new InvalidOperationException("Empty payload");

			var raw = JsonSerializer.Deserialize<ProjectPayload>(incoming.Payload!, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
			if (raw == null) throw new InvalidOperationException("Invalid payload");

			await using var tx = await _db.Database.BeginTransactionAsync(ct);

			var projects = _db.Set<ProjectReadModel>();
			var existing = await projects.FindAsync(new object?[] { raw.AggregateId }, ct);

			if (incoming.IsTombstone || raw.IsTombstone || (raw.EventType?.EndsWith("deleted") == true))
			{
				if (existing != null)
					projects.Remove(existing);
			}
			else
			{
				if (existing == null)
				{
					projects.Add(new ProjectReadModel
					{
						Id = raw.AggregateId,
						Title = raw.Title ?? "",
						OwnerId = raw.OwnerId,
					});
				}
				else
				{
					var needUpdate = false;

					if (!string.Equals(existing.Title, raw.Title ?? "", StringComparison.Ordinal))
					{
						existing.Title = raw.Title ?? "";
						needUpdate = true;
					}

					if (existing.OwnerId != raw.OwnerId)
					{
						existing.OwnerId = raw.OwnerId;
						needUpdate = true;
					}

					if (needUpdate)
					{
						_db.Update(existing); 
					}
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


		private class ProjectPayload
		{
			public Guid AggregateId { get; set; }
			public Guid ProjectId { get; set; }
			public string? Title { get; set; }
			public Guid OwnerId { get; set; }
			public string? EventType { get; set; }
			public int Version { get; set; }
			public bool IsTombstone { get; set; }
		}
	}
}
