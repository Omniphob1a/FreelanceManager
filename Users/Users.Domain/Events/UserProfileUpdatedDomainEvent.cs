using Tasks.Domain.Aggregate.Events;
using Users.Domain.Interfaces;

namespace Users.Domain.Events
{
	public record UserProfileUpdatedDomainEvent(Guid UserId) : DomainEvent(UserId);
}
