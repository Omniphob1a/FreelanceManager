
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Users.Application.Interfaces;
using Users.Infrastructure.Data;
using Users.Infrastructure.Models;

namespace Users.Infrastructure.Outbox
{
	public class OutboxService : IOutboxService
	{
		private readonly UsersDbContext _db;
		private readonly JsonSerializerOptions _jsonOptions;

		public OutboxService(UsersDbContext db)
		{
			_db = db;
			_jsonOptions = new JsonSerializerOptions
			{
				PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
				WriteIndented = false,
				DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
			};
		}

		public Task Add(object message, string topic, string? key = null, CancellationToken ct = default)
		{
			if (message == null) throw new ArgumentNullException(nameof(message));
			if (string.IsNullOrWhiteSpace(topic)) throw new ArgumentException("Topic is required", nameof(topic));

			var payload = JsonSerializer.Serialize(message, _jsonOptions);

			var outbox = new OutboxMessage
			{
				Topic = topic,
				Key = key,
				Payload = payload,
				OccurredAt = DateTime.UtcNow
			};

			_db.OutboxMessages.Add(outbox);

			return Task.CompletedTask;
		}

		public Task AddTombstone(string topic, string? key = null, CancellationToken ct = default)
		{
			if (string.IsNullOrWhiteSpace(topic)) throw new ArgumentException("topic");
			if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("key");

			var outbox = new OutboxMessage
			{
				Topic = topic,
				Key = key,
				Payload = null!, 
				OccurredAt = DateTime.UtcNow
			};

			_db.OutboxMessages.Add(outbox);
			return Task.CompletedTask;
		}
	}
}
