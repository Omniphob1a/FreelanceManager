using Notifications.Domain.Common;

namespace Notifications.Domain.Aggregates.Notification.Events
{
	public record NotificationReadDomainEvent(
		Guid AggregateId,
		Guid NotificationId,
		Guid UserId,
		DateTimeOffset ReadAt
	) : DomainEvent(AggregateId);
}
