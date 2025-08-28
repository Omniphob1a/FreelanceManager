using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projects.Domain.Events
{
	public record ProjectMemberRoleChangedDomainEvent(Guid ProjectId, Guid MemberId, Guid UserId, string NewRole) : DomainEvent(ProjectId);
}
