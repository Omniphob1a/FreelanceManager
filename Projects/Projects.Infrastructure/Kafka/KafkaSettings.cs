using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projects.Infrastructure.Kafka
{
	public class KafkaSettings
	{
		public string BootstrapServers { get; init; } = "localhost:9092";
		public string GroupId { get; set; } = string.Empty;
		public string AutoOffsetReset { get; set; } = "Earliest";
		public bool EnableAutoCommit { get; set; } = false;
		public string SecurityProtocol { get; set; } = string.Empty;
		public string SaslMechanism { get; set; } = string.Empty;
		public string SaslUsername { get; set; } = string.Empty;
		public string SaslPassword { get; set; } = string.Empty;
		public ConfluentConfigOptions Options { get; init; } = new();

	}
}
