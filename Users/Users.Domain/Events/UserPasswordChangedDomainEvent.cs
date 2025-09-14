using Tasks.Domain.Aggregate.Events;
using Users.Domain.Interfaces;

namespace Users.Domain.Events
{
	public record UserPasswordChangedDomainEvent(Guid UserId) : DomainEvent(UserId);
}
