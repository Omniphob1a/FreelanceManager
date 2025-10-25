using Notifications.Domain.Aggregates.Notification.Enums;
using Notifications.Domain.Common;

namespace Notifications.Domain.Aggregates.Notification.Events
{
	public record DeliveryDeadDomainEvent(
		Guid AggregateId,
		Guid DeliveryId,
		NotificationChannel Channel,
		string Reason
	) : DomainEvent(AggregateId);
}
