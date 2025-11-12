using System.Threading.Tasks;
using Tasks.Domain.Aggregate.Events;
using Users.Domain.Entities;
using Users.Domain.Interfaces;

namespace Users.Domain.Events
{
	public record UserDeletedDomainEvent(Guid UserId) : DomainEvent(UserId, nameof(User))
	{
		public override string EventType => "users.removed";
		public override string? KafkaTopic => "users";
		public override string? KafkaKey => UserId.ToString();
		public override bool IsTombstone => true;
	}
}
