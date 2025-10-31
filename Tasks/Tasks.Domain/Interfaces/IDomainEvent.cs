using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tasks.Domain.Interfaces
{
	public interface IDomainEvent
	{
		DateTime OccurredOnUtc { get; }
		Guid EventId { get; }
		Guid AggregateId { get; }
		string AggregateType { get; }
		string EventType { get; }
		int Version { get; }
		string? KafkaTopic { get; }
		string? KafkaKey { get; }
		bool IsTombstone { get; }
		IReadOnlyDictionary<string, string>? KafkaHeaders { get; }

	}
}
