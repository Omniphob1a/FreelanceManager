using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Users.Application.DTOs;
using Users.Application.Events;
using Users.Application.Projects.Commands.ConfirmProject; // <- убедись, что namespace команды совпадает
using Users.Infrastructure.Data;

namespace Users.Infrastructure.Processors
{
	public class ConfirmRequestProcessor : IIncomingEventProcessor
	{
		private const int MaxRetries = 5;
		private readonly UsersDbContext _db;
		private readonly IMediator _mediator;

		public IReadOnlyCollection<string> SupportedEventTypes { get; } =
			new[] { "projects.confirm-requested" }; // событие, которое реально приходит

		public ConfirmRequestProcessor(UsersDbContext db, IMediator mediator)
		{
			_db = db ?? throw new ArgumentNullException(nameof(db));
			_mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
		}

		public async Task HandleAsync(IncomingEventDto incoming, CancellationToken ct)
		{
			if (string.IsNullOrWhiteSpace(incoming.Payload))
				throw new InvalidOperationException("Empty payload");

			using var doc = JsonDocument.Parse(incoming.Payload!);
			var root = doc.RootElement;

			if (!root.TryGetProperty("projectId", out var projProp) ||
				projProp.ValueKind != JsonValueKind.String ||
				!Guid.TryParse(projProp.GetString(), out var projectId))
			{
				throw new InvalidOperationException("projectId missing or invalid in payload");
			}

			if (!root.TryGetProperty("userId", out var userProp) ||
				userProp.ValueKind != JsonValueKind.String ||
				!Guid.TryParse(userProp.GetString(), out var userId))
			{
				throw new InvalidOperationException("userId missing or invalid in payload");
			}
			var cmd = new ConfirmProjectCommand(projectId, userId);
			var result = await _mediator.Send(cmd, ct);

			var incomingEntity = await _db.IncomingEvents.FindAsync(new object?[] { incoming.Id }, ct);
			if (result.IsFailed)
			{
				if (incomingEntity != null)
				{
					incomingEntity.RetryCount += 1;
					incomingEntity.LastError = string.Join("; ", result.Errors.Select(e => e.Message));

					if (incomingEntity.RetryCount >= MaxRetries)
					{
						incomingEntity.Processed = true;
						incomingEntity.ProcessedAt = DateTimeOffset.UtcNow;
					}
					else
					{
						var delaySeconds = Math.Pow(2, Math.Min(incomingEntity.RetryCount, 6));
						incomingEntity.NextAttemptAt = DateTimeOffset.UtcNow.AddSeconds(delaySeconds);
					}
				}
				await _db.SaveChangesAsync(ct);
				return; 
			}

			if (incomingEntity != null)
			{
				incomingEntity.Processed = true;
				incomingEntity.ProcessedAt = DateTimeOffset.UtcNow;
				await _db.SaveChangesAsync(ct);
			}
		}
	}
}
