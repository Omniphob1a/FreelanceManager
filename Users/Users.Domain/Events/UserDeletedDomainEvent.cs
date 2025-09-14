using Tasks.Domain.Aggregate.Events;
using Users.Domain.Interfaces;

namespace Users.Domain.Events
{
	public record UserDeletedDomainEvent(Guid UserId) : DomainEvent(UserId);
}
