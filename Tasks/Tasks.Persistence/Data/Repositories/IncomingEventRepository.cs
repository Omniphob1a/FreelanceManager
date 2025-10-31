using Microsoft.EntityFrameworkCore;
using Tasks.Application.DTOs;
using Tasks.Application.Interfaces;
using Tasks.Persistence.Models.ReadModels;

namespace Tasks.Persistence.Data.Repositories
{
	public class IncomingEventRepository : IIncomingEventRepository
	{
		private readonly ProjectTasksDbContext _db;
		public IncomingEventRepository(ProjectTasksDbContext db) => _db = db;

		public async Task<List<IncomingEventDto>> GetPendingAsync(int batchSize, CancellationToken ct)
		{
			var now = DateTimeOffset.UtcNow;
			return await _db.IncomingEvents
				.Where(e => !e.Processed && e.NextAttemptAt <= now)
				.OrderBy(e => e.OccurredAt)
				.Take(batchSize)
				.Select(e => new IncomingEventDto
				{
					Id = e.Id,
					EventId = e.EventId,
					AggregateId = e.AggregateId,
					AggregateType = e.AggregateType,
					EventType = e.EventType,
					Payload = e.Payload,
					OccurredAt = e.OccurredAt,
					IsTombstone = e.IsTombstone,
					RetryCount = e.RetryCount,
					NextAttemptAt = e.NextAttemptAt
				})
				.ToListAsync(ct);
		}

		public async Task MarkProcessedAsync(Guid id, CancellationToken ct)
		{
			var ent = new IncomingEvent { Id = id };
			_db.IncomingEvents.Attach(ent);
			ent.Processed = true;
			ent.ProcessedAt = DateTimeOffset.UtcNow;
			_db.Entry(ent).Property(x => x.Processed).IsModified = true;
			_db.Entry(ent).Property(x => x.ProcessedAt).IsModified = true;
			await _db.SaveChangesAsync(ct);
		}

		public async Task IncrementRetryAsync(Guid id, string lastError, DateTimeOffset nextAttemptAt, CancellationToken ct)
		{
			var ent = await _db.IncomingEvents.FindAsync(new object[] { id }, ct);
			if (ent == null) return;
			ent.RetryCount++;
			ent.LastError = lastError;
			ent.NextAttemptAt = nextAttemptAt;
			await _db.SaveChangesAsync(ct);
		}
	}
}
