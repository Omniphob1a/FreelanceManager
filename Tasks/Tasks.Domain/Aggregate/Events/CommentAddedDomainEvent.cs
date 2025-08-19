using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tasks.Domain.Aggregate.Events
{
	public record CommentAddedDomainEvent(Guid TaskId, Guid EntryId) : DomainEvent(TaskId);
}
