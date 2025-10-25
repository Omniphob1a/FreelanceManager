using Notifications.Domain.Aggregates.Notification.Enums;
using Notifications.Domain.Common;

namespace Notifications.Domain.Aggregates.Notification.Events
{
	public record DeliveryScheduledDomainEvent(
		Guid AggregateId,
		Guid DeliveryId,
		NotificationChannel Channel,
		DateTimeOffset ScheduledAt
	) : DomainEvent(AggregateId);
}
