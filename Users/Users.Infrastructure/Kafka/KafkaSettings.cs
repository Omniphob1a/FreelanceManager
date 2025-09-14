using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Users.Infrastructure.Kafka
{
	public class KafkaSettings
	{
		public string BootstrapServers { get; init; } = "localhost:9092";
		public ConfluentConfigOptions Options { get; init; } = new();
	}
}
