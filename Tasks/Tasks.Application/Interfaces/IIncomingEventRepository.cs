using Tasks.Application.DTOs;

namespace Tasks.Application.Interfaces
{
	public interface IIncomingEventRepository
	{
		Task<List<IncomingEventDto>> GetPendingAsync(int batchSize, CancellationToken ct);
		Task MarkProcessedAsync(Guid id, CancellationToken ct);
		Task IncrementRetryAsync(Guid id, string lastError, DateTimeOffset nextAttemptAt, CancellationToken ct);
	}
}
