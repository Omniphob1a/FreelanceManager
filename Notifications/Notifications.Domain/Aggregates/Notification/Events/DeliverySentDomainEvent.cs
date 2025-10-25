using Notifications.Domain.Aggregates.Notification.Enums;
using Notifications.Domain.Common;

namespace Notifications.Domain.Aggregates.Notification.Events
{
	public record DeliverySentDomainEvent(
		Guid AggregateId,
		Guid DeliveryId,
		NotificationChannel Channel,
		DateTimeOffset SentAt,
		string? ProviderMessageId
	) : DomainEvent(AggregateId);
}
