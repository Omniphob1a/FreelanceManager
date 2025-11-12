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
	public class UserEventsProcessor : IIncomingEventProcessor
	{
		private readonly ProjectTasksDbContext _db;

		public IReadOnlyCollection<string> SupportedEventTypes { get; } =
			new[] { "users." };

		public UserEventsProcessor(ProjectTasksDbContext db) => _db = db;

		public async Task HandleAsync(IncomingEventDto incoming, CancellationToken ct)
		{
			if (string.IsNullOrWhiteSpace(incoming.Payload))
				throw new InvalidOperationException("Empty payload");

			using var doc = JsonDocument.Parse(incoming.Payload!);
			var root = doc.RootElement;

			// Id обязателен
			Guid id;
			if (root.TryGetProperty("userId", out var idProp) &&
				idProp.ValueKind == JsonValueKind.String &&
				Guid.TryParse(idProp.GetString(), out var parsedId))
			{
				id = parsedId;
			}
			else
			{
				throw new InvalidOperationException("Id missing in payload");
			}

			bool isDelete = incoming.IsTombstone;
			if (root.TryGetProperty("isTombstone", out var tomb) && tomb.ValueKind == JsonValueKind.True) isDelete = true;
			if (root.TryGetProperty("eventType", out var evt) && evt.ValueKind == JsonValueKind.String && evt.GetString()!.EndsWith("deleted", StringComparison.OrdinalIgnoreCase)) isDelete = true;

			await using var tx = await _db.Database.BeginTransactionAsync(ct);

			var users = _db.Users;
			var existing = await users.Where(u => u.Id == id).FirstOrDefaultAsync(ct);

			if (isDelete)
			{
				if (existing != null) users.Remove(existing);
			}
			else
			{
				if (existing == null)
				{
					// Create: read only present fields
					string name = "";
					string login = "";
					int gender = 0;
					DateTime birthday = DateTime.MinValue;

					if (root.TryGetProperty("name", out var nameProp) && nameProp.ValueKind == JsonValueKind.String)
						name = nameProp.GetString() ?? "";

					if (root.TryGetProperty("login", out var loginProp) && loginProp.ValueKind == JsonValueKind.String)
						login = loginProp.GetString() ?? "";

					if (root.TryGetProperty("gender", out var genderProp) && genderProp.ValueKind == JsonValueKind.Number && genderProp.TryGetInt32(out var g))
						gender = g;

					if (root.TryGetProperty("birthday", out var bProp) && bProp.ValueKind == JsonValueKind.String && DateTime.TryParse(bProp.GetString(), out var dt))
					{
						birthday = dt.Kind == DateTimeKind.Local ? dt.ToUniversalTime() : DateTime.SpecifyKind(dt, DateTimeKind.Utc);
					}

					await users.AddAsync(new UserReadModel
					{
						Id = id,
						Name = name,
						Login = login,
						Gender = gender,
						Birthday = birthday
					}, ct);
				}
				else
				{
					// Update only present fields
					bool updated = false;

					if (root.TryGetProperty("name", out var nameProp) && nameProp.ValueKind == JsonValueKind.String)
					{
						var newName = nameProp.GetString() ?? "";
						if (!string.Equals(existing.Name, newName, StringComparison.Ordinal))
						{
							existing.Name = newName;
							updated = true;
						}
					}

					if (root.TryGetProperty("login", out var loginProp) && loginProp.ValueKind == JsonValueKind.String)
					{
						var newLogin = loginProp.GetString() ?? "";
						if (!string.Equals(existing.Login, newLogin, StringComparison.Ordinal))
						{
							existing.Login = newLogin;
							updated = true;
						}
					}

					if (root.TryGetProperty("gender", out var genderProp) && genderProp.ValueKind == JsonValueKind.Number && genderProp.TryGetInt32(out var newGender))
					{
						if (existing.Gender != newGender)
						{
							existing.Gender = newGender;
							updated = true;
						}
					}

					if (root.TryGetProperty("birthday", out var bProp) && bProp.ValueKind == JsonValueKind.String && DateTime.TryParse(bProp.GetString(), out var newBirthday))
					{
						var normalized = newBirthday.Kind == DateTimeKind.Local ? newBirthday.ToUniversalTime() : DateTime.SpecifyKind(newBirthday, DateTimeKind.Utc);
						if (existing.Birthday != normalized)
						{
							existing.Birthday = normalized;
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
