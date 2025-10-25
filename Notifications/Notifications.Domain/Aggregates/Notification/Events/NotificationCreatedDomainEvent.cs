using Notifications.Domain.Common;

namespace Notifications.Domain.Aggregates.Notification.Events
{
	public record NotificationCreatedDomainEvent(
		Guid AggregateId,
		Guid NotificationId,
		Guid UserId,
		string TemplateKey,
		string? Payload,
		IEnumerable<DeliveryInfo> Deliveries
	) : DomainEvent(AggregateId);
}
