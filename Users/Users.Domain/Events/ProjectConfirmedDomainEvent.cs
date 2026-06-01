using Tasks.Domain.Aggregate.Events;
using Users.Domain.Entities;

namespace Users.Domain.Events
{
	public record ProjectConfirmedDomainEvent(
		Guid ProjectId,
		Guid UserId,
		DateTime ConfirmedAt,
		int RegisteredObjects
	) : DomainEvent(ProjectId, nameof(User))
	{
		public override string EventType => "confirm.processed";
		public override string? KafkaTopic => "object-confirm-responses";
		public override string? KafkaKey => ProjectId.ToString();
	}
}
