using Users.Application.DTOs;

namespace Users.Application.Events
{
	public interface IIncomingEventProcessor
	{
		IReadOnlyCollection<string> SupportedEventTypes { get; }
		Task HandleAsync(IncomingEventDto incoming, CancellationToken ct);
	}
}
