using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projects.Application.DTOs
{
	public class IncomingEventDto
	{
		public Guid Id { get; init; }
		public Guid? EventId { get; init; }
		public Guid? AggregateId { get; init; }
		public string AggregateType { get; init; } = default!;
		public string EventType { get; init; } = default!;
		public string? Payload { get; init; }
		public DateTimeOffset OccurredAt { get; init; }
		public bool IsTombstone { get; init; }
		public int RetryCount { get; init; }
		public DateTimeOffset NextAttemptAt { get; init; }
	}
}
