using Tasks.Domain.Aggregate.Events;
using Users.Domain.Interfaces;
using Users.Domain.ValueObjects;

namespace Users.Domain.Events
{
	public record UserRegisteredDomainEvent(Guid UserId, string Login, Email Email) : DomainEvent(UserId);
}
