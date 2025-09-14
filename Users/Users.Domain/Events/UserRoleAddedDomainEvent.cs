using Tasks.Domain.Aggregate.Events;
using Users.Domain.Interfaces;

namespace Users.Domain.Events
{
	public record UserRoleAddedDomainEvent(Guid UserId, Guid RoleId) : DomainEvent(UserId);
}
