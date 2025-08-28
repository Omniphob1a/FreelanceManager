using Confluent.Kafka;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projects.Infrastructure.Kafka
{
	public class ConfluentKafkaProducer : IKafkaProducer, IDisposable
	{
		private readonly IProducer<string, string> _producer;

		public ConfluentKafkaProducer(KafkaSettings settings)
		{
			var cfg = new ProducerConfig
			{
				BootstrapServers = settings.BootstrapServers,
				Acks = Acks.All,
				EnableIdempotence = settings.Options.EnableIdempotence
			};

			_producer = new ProducerBuilder<string, string>(cfg).Build();
		}

		public async Task ProduceAsync(string topic, string? key, string payload, CancellationToken ct = default)
		{
			var msg = new Message<string, string> { Key = key, Value = payload };
			var result = await _producer.ProduceAsync(topic, msg, ct);
		}

		public void Dispose() => _producer?.Dispose();
	}
}
