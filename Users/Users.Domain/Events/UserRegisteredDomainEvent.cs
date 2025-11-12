using Tasks.Domain.Aggregate.Events;
using Users.Domain.Entities;
using Users.Domain.Interfaces;
using Users.Domain.ValueObjects;

namespace Users.Domain.Events
{
	public record UserRegisteredDomainEvent(Guid UserId, string Login, string Name, DateTime Birthday, int Gender) : DomainEvent(UserId, nameof(User))
	{
		public override string EventType => "users.created";
		public override string? KafkaTopic => "users";
		public override string? KafkaKey => UserId.ToString();
	}
}
