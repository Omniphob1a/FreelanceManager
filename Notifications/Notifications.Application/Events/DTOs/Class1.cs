using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Notifications.Application.Events.DTOs
{
	public record TaskAssignedEvent(Guid TaskId, Guid AssigneeId, Guid AggregateId, Guid EventId);

}
