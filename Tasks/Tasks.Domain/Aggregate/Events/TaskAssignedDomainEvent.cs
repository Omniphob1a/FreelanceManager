using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tasks.Domain.Aggregate.Events
{
	public record TaskAssignedDomainEvent(Guid TaskId, Guid AssigneeId) : DomainEvent(TaskId);
}
