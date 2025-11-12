using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Tasks.Application.DTOs;
using Tasks.Application.Events;
using Tasks.Persistence.Data;
using Tasks.Persistence.Models.ReadModels;

namespace Tasks.Infrastructure.Processors
{
	public class MemberEventsProcessor : IIncomingEventProcessor
	{
		private readonly ProjectTasksDbContext _db;

		public IReadOnlyCollection<string> SupportedEventTypes { get; } =
			new[] { "projects.member" };

		public MemberEventsProcessor(ProjectTasksDbContext db) => _db = db;

		public async Task HandleAsync(IncomingEventDto incoming, CancellationToken ct)
		{
			if (string.IsNullOrWhiteSpace(incoming.Payload))
				throw new InvalidOperationException("Empty payload");

			using var doc = JsonDocument.Parse(incoming.Payload!);
			var root = doc.RootElement;

			if (!(root.TryGetProperty("aggregateId", out var aggProp) && aggProp.ValueKind == JsonValueKind.String && Guid.TryParse(aggProp.GetString(), out var projectId)))
				throw new InvalidOperationException("aggregateId missing");

			if (!(root.TryGetProperty("memberId", out var memProp) && memProp.ValueKind == JsonValueKind.String && Guid.TryParse(memProp.GetString(), out var memberId)))
				throw new InvalidOperationException("memberId missing");

			bool isDelete = incoming.IsTombstone;
			if (root.TryGetProperty("isTombstone", out var tomb) && tomb.ValueKind == JsonValueKind.True) isDelete = true;
			if (root.TryGetProperty("eventType", out var evt) && evt.ValueKind == JsonValueKind.String && evt.GetString()!.EndsWith("deleted", StringComparison.OrdinalIgnoreCase)) isDelete = true;

			await using var tx = await _db.Database.BeginTransactionAsync(ct);

			var members = _db.ProjectMembers;
			var existing = await members.Where(m => m.Id == memberId && m.ProjectId == projectId).FirstOrDefaultAsync(ct);

			if (isDelete)
			{
				if (existing != null) members.Remove(existing);
			}
			else
			{
				if (existing == null)
				{
					// Create: read only present fields
					Guid userId = Guid.Empty;
					string role = "";
					DateTime addedAt = DateTime.UtcNow;

					if (root.TryGetProperty("userId", out var uProp) && uProp.ValueKind == JsonValueKind.String && Guid.TryParse(uProp.GetString(), out var parsedU))
						userId = parsedU;

					if (root.TryGetProperty("role", out var rProp) && rProp.ValueKind == JsonValueKind.String)
						role = rProp.GetString() ?? "";

					if (root.TryGetProperty("addedAt", out var aProp) && aProp.ValueKind == JsonValueKind.String && DateTime.TryParse(aProp.GetString(), out var parsedA))
						addedAt = parsedA;

					await members.AddAsync(new MemberReadModel
					{
						Id = memberId,
						ProjectId = projectId,
						UserId = userId,
						Role = role,
						AddedAt = addedAt
					}, ct);
				}
				else
				{
					bool updated = false;

					if (root.TryGetProperty("userId", out var uProp) && uProp.ValueKind == JsonValueKind.String && Guid.TryParse(uProp.GetString(), out var newUser))
					{
						if (existing.UserId != newUser)
						{
							existing.UserId = newUser;
							updated = true;
						}
					}

					if (root.TryGetProperty("role", out var rProp) && rProp.ValueKind == JsonValueKind.String)
					{
						var newRole = rProp.GetString() ?? "";
						if (!string.Equals(existing.Role, newRole, StringComparison.Ordinal))
						{
							existing.Role = newRole;
							updated = true;
						}
					}

					if (root.TryGetProperty("addedAt", out var aProp) && aProp.ValueKind == JsonValueKind.String && DateTime.TryParse(aProp.GetString(), out var newAdded))
					{
						if (existing.AddedAt != newAdded)
						{
							existing.AddedAt = newAdded;
							updated = true;
						}
					}

					if (updated) _db.Update(existing);
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
