using Notifications.Domain.Aggregates.Notification.Enums;

namespace Notifications.Domain.Aggregates.Notification.Events
{
	public record DeliveryInfo(Guid DeliveryId, NotificationChannel Channel, DateTimeOffset? ScheduledAt);
}
