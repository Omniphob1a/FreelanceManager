using Confluent.Kafka;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projects.Infrastructure.Kafka
{
	public interface IKafkaProducer
	{
		Task ProduceAsync(string topic, string key, string payload, CancellationToken ct);
	}
}
