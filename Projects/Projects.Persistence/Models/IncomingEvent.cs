using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projects.Persistence.Models.ReadModels
{
	public class IncomingEvent
	{
		public Guid Id { get; set; } = Guid.NewGuid();

		public Guid? EventId { get; set; }         
		public Guid? AggregateId { get; set; }
		public string AggregateType { get; set; } = "";
		public string EventType { get; set; } = "";
		public string? Topic { get; set; }
		public string? Key { get; set; }

		public string? Payload { get; set; }

		public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
		public bool IsTombstone { get; set; } = false;

		public bool Processed { get; set; } = false;
		public DateTimeOffset? ProcessedAt { get; set; }
		public int RetryCount { get; set; } = 0;
		public string? LastError { get; set; }
		public DateTimeOffset NextAttemptAt { get; set; } = DateTimeOffset.UtcNow;
	}
}
