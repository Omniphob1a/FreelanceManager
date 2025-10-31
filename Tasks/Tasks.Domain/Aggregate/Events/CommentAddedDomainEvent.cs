using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tasks.Domain.Aggregate.Root;

namespace Tasks.Domain.Aggregate.Events
{
	public record CommentAddedDomainEvent(Guid TaskId, Guid CommentId) : DomainEvent(TaskId, nameof(ProjectTask))
	{
		public override string EventType => "tasks.comment.added";
		public override string? KafkaTopic => "tasks";
		public override string? KafkaKey => $"{TaskId}-milestone-{CommentId}";
	}
}
