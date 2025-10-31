using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tasks.Domain.Aggregate.Root;

namespace Tasks.Domain.Aggregate.Events
{
	public record TaskCreatedDomainEvent(Guid TaskId, Guid ProjectId) : DomainEvent(TaskId, nameof(ProjectTask))
	{
		public override string EventType => "tasks.created";
		public override string? KafkaTopic => "tasks";
		public override string? KafkaKey => TaskId.ToString();
	}
}
