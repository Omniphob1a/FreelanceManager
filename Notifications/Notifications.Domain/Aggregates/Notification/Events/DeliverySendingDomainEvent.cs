using Notifications.Domain.Aggregates.Notification.Enums;
using Notifications.Domain.Common;

namespace Notifications.Domain.Aggregates.Notification.Events
{
	public record DeliverySendingDomainEvent(
		Guid AggregateId,
		Guid DeliveryId,
		NotificationChannel Channel
	) : DomainEvent(AggregateId);
}
