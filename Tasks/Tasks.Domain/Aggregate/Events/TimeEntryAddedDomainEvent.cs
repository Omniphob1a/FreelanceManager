using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tasks.Domain.Aggregate.Root;

namespace Tasks.Domain.Aggregate.Events
{
	public record TimeEntryAddedDomainEvent(Guid TaskId, Guid EntryId) : DomainEvent(TaskId, nameof(ProjectTask))
	{
		public override string EventType => "tasks.time_entry.added";
		public override string? KafkaTopic => "tasks";
		public override string? KafkaKey => $"{TaskId}-time_entry-{EntryId}";
	}
}
