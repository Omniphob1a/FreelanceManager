using Tasks.Domain.Aggregate.Events;
using Users.Domain.Entities;
using Users.Domain.Interfaces;

namespace Users.Domain.Events
{
	public record UserRoleRemovedDomainEvent(Guid UserId, Guid RoleId) : DomainEvent(UserId, nameof(User))
	{
		public override string EventType => "users.role.added";
		public override string? KafkaTopic => "users";
		public override string? KafkaKey => $"{UserId}-role-{RoleId}";
	}
}
