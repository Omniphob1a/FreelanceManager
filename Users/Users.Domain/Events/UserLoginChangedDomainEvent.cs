using Tasks.Domain.Aggregate.Events;
using Users.Domain.Entities;
using Users.Domain.Interfaces;

namespace Users.Domain.Events
{
	public record UserLoginChangedDomainEvent(Guid UserId, string NewLogin) : DomainEvent(UserId, nameof(User))
	{
		public override string EventType => "users.login_changed";
		public override string? KafkaTopic => "users";
		public override string? KafkaKey => UserId.ToString();
	}
}
