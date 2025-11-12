using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Users.Domain.Interfaces;

namespace Tasks.Domain.Aggregate.Events
{
	public abstract record DomainEvent(Guid AggregateId, string AggregateType) : IDomainEvent
	{
		public Guid EventId { get; } = Guid.NewGuid();
		public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
		public string AggregateType { get; init; } = AggregateType;
		public virtual string EventType => GetType().FullName!;
		public virtual int Version => 1;
		public virtual string? KafkaTopic => null;
		public virtual string? KafkaKey => null;
		public virtual bool IsTombstone => false;
		public virtual IReadOnlyDictionary<string, string>? KafkaHeaders => null;
	}
}
