using Tasks.Domain.Aggregate.Events;
using Users.Domain.Interfaces;

namespace Users.Domain.Events
{
	public record UserLoginChangedDomainEvent(Guid UserId, string NewLogin) : DomainEvent(UserId);
}
