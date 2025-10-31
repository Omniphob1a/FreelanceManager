using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tasks.Domain.Aggregate.Root;

namespace Tasks.Domain.Aggregate.Events
{
	public record TaskCanceledDomainEvent(Guid TaskId, string Reason) : DomainEvent(TaskId, nameof(ProjectTask))
	{
		public override string EventType => "tasks.canceled";
		public override string? KafkaTopic => "tasks";
		public override string? KafkaKey => TaskId.ToString();
	}
}
