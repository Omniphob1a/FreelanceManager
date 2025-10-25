using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Notifications.Infrastructure.Kafka
{
	public class KafkaSettings
	{
		public string BootstrapServers { get; set; } = string.Empty;
		public string GroupId { get; set; } = string.Empty;
		public string AutoOffsetReset { get; set; } = "Earliest";
		public bool EnableAutoCommit { get; set; } = false; 
	}
}
