using Notifications.Domain.Aggregates.Notification.Enums;
using Notifications.Domain.Common;

namespace Notifications.Domain.Aggregates.Notification.Events
{
	// Ошибка при доставке
	public record DeliveryFailedDomainEvent(
		Guid AggregateId,
		Guid DeliveryId,
		NotificationChannel Channel,
		int Attempts,
		string Error
	) : DomainEvent(AggregateId);
}
