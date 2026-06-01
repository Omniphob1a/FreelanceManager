using Microsoft.EntityFrameworkCore;
using Projects.Application.DTOs;
using Projects.Persistence.Data;
using Projects.Persistence.Models.ReadModels;
using System.Text.Json;
using Tasks.Application.Events;

namespace Projects.Infrastructure.Processors
{
	public class UserEventsProcessor : IIncomingEventProcessor
	{
		private readonly ProjectsDbContext _db;

		public IReadOnlyCollection<string> SupportedEventTypes { get; } = new[]
		{
			"users.created",
			"users.profile_changed",
			"users.login_changed",
			"users.password_changed",
			"users.role.added",
			"users.role.removed",
			"users.restored",
			"users.removed"
		};

		public UserEventsProcessor(ProjectsDbContext db) => _db = db;

		public async Task HandleAsync(IncomingEventDto incoming, CancellationToken ct)
		{
			switch (incoming.EventType)
			{
				case "users.created":
					await UpsertCreatedUserAsync(incoming, ct);
					break;
				case "users.profile_changed":
					await UpdateProfileAsync(incoming, ct);
					break;
				case "users.login_changed":
					await UpdateLoginAsync(incoming, ct);
					break;
				case "users.removed":
					await RemoveUserAsync(incoming, ct);
					break;
			}

			await MarkProcessedAsync(incoming.Id, ct);
		}

		private async Task UpsertCreatedUserAsync(IncomingEventDto incoming, CancellationToken ct)
		{
			using var doc = ParsePayload(incoming);
			var root = doc.RootElement;
			var userId = ReadGuid(root, "userId") ?? incoming.AggregateId;
			if (!userId.HasValue)
				throw new InvalidOperationException("userId missing");

			var user = await _db.Users.FirstOrDefaultAsync(x => x.Id == userId.Value, ct);
			if (user == null)
			{
				user = new UserReadModel { Id = userId.Value };
				_db.Users.Add(user);
			}

			user.Login = ReadString(root, "login") ?? user.Login;
			user.Name = ReadString(root, "name") ?? user.Name;
			user.Gender = ReadInt(root, "gender") ?? user.Gender;
			user.Birthday = ReadDateTime(root, "birthday") ?? user.Birthday;
		}

		private async Task UpdateProfileAsync(IncomingEventDto incoming, CancellationToken ct)
		{
			using var doc = ParsePayload(incoming);
			var root = doc.RootElement;
			var userId = ReadGuid(root, "userId") ?? incoming.AggregateId;
			if (!userId.HasValue)
				throw new InvalidOperationException("userId missing");

			var user = await _db.Users.FirstOrDefaultAsync(x => x.Id == userId.Value, ct);
			if (user == null)
			{
				user = new UserReadModel { Id = userId.Value };
				_db.Users.Add(user);
			}

			user.Name = ReadString(root, "newName") ?? user.Name;
			user.Gender = ReadInt(root, "newGender") ?? user.Gender;
			user.Birthday = ReadDateTime(root, "newBirthday") ?? user.Birthday;
		}

		private async Task UpdateLoginAsync(IncomingEventDto incoming, CancellationToken ct)
		{
			using var doc = ParsePayload(incoming);
			var root = doc.RootElement;
			var userId = ReadGuid(root, "userId") ?? incoming.AggregateId;
			if (!userId.HasValue)
				throw new InvalidOperationException("userId missing");

			var user = await _db.Users.FirstOrDefaultAsync(x => x.Id == userId.Value, ct);
			if (user != null)
				user.Login = ReadString(root, "newLogin") ?? user.Login;
		}

		private async Task RemoveUserAsync(IncomingEventDto incoming, CancellationToken ct)
		{
			var userId = incoming.AggregateId;
			if (!userId.HasValue && !string.IsNullOrWhiteSpace(incoming.Payload))
			{
				using var doc = JsonDocument.Parse(incoming.Payload);
				userId = ReadGuid(doc.RootElement, "userId");
			}

			if (!userId.HasValue)
				return;

			var user = await _db.Users.FirstOrDefaultAsync(x => x.Id == userId.Value, ct);
			if (user != null)
				_db.Users.Remove(user);
		}

		private async Task MarkProcessedAsync(Guid incomingId, CancellationToken ct)
		{
			var incoming = await _db.IncomingEvents.FindAsync(new object?[] { incomingId }, ct);
			if (incoming != null)
			{
				incoming.Processed = true;
				incoming.ProcessedAt = DateTimeOffset.UtcNow;
				incoming.LastError = null;
			}

			await _db.SaveChangesAsync(ct);
		}

		private static JsonDocument ParsePayload(IncomingEventDto incoming)
		{
			if (string.IsNullOrWhiteSpace(incoming.Payload))
				throw new InvalidOperationException("Empty payload");

			return JsonDocument.Parse(incoming.Payload);
		}

		private static string? ReadString(JsonElement root, string propertyName)
			=> root.TryGetProperty(propertyName, out var prop) && prop.ValueKind == JsonValueKind.String
				? prop.GetString()
				: null;

		private static int? ReadInt(JsonElement root, string propertyName)
			=> root.TryGetProperty(propertyName, out var prop) && prop.ValueKind == JsonValueKind.Number && prop.TryGetInt32(out var value)
				? value
				: null;

		private static Guid? ReadGuid(JsonElement root, string propertyName)
			=> root.TryGetProperty(propertyName, out var prop) &&
			   prop.ValueKind == JsonValueKind.String &&
			   Guid.TryParse(prop.GetString(), out var value)
				? value
				: null;

		private static DateTime? ReadDateTime(JsonElement root, string propertyName)
			=> root.TryGetProperty(propertyName, out var prop) &&
			   prop.ValueKind == JsonValueKind.String &&
			   DateTime.TryParse(prop.GetString(), null, System.Globalization.DateTimeStyles.AssumeUniversal | System.Globalization.DateTimeStyles.AdjustToUniversal, out var value)
				? DateTime.SpecifyKind(value, DateTimeKind.Utc)
				: null;
	}
}
