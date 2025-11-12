using Projects.Application.DTOs;

namespace Tasks.Application.Events
{
	public interface IIncomingEventProcessor
	{
		IReadOnlyCollection<string> SupportedEventTypes { get; }
		Task HandleAsync(IncomingEventDto incoming, CancellationToken ct);
	}
}
