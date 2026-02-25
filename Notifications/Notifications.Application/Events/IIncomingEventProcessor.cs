using Notifications.Application.DTOs;

namespace Notifications.Application.Events
{
	public interface IIncomingEventProcessor
	{
		IReadOnlyCollection<string> SupportedEventTypes { get; }
		Task HandleAsync(IncomingEventDto incoming, CancellationToken ct);
	}
}
