using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Users.Infrastructure.Models
{
	public class OutboxMessage
	{
		public Guid Id { get; set; } = Guid.NewGuid();
		public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
		public string Topic { get; set; } = default!;
		public string? Key { get; set; }
		public string? Payload { get; set; } = default!;
		public bool Processed { get; set; } = false;
		public DateTime? ProcessedAt { get; set; }
		public int RetryCount { get; set; } = 0;
		public string? LastError { get; set; }
	}
}
