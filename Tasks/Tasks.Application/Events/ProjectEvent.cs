using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tasks.Application.Events
{
	public class ProjectEvent
	{
		public Guid ProjectId { get; set; }
		public string Title { get; set; } = string.Empty;
		public Guid OwnerId { get; set; }

		public string EventType { get; set; } = string.Empty;
		public string KafkaTopic { get; set; } = string.Empty;
		public string KafkaKey { get; set; } = string.Empty;
		public Guid AggregateId { get; set; }
		public Guid EventId { get; set; }
		public DateTime OccurredOnUtc { get; set; }
		public string AggregateType { get; set; } = string.Empty;
		public int Version { get; set; }
		public bool IsTombstone { get; set; }
	}
}
