using Confluent.Kafka;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Projects.Infrastructure.Kafka
{
	public class ConfluentKafkaProducer : IKafkaProducer, IDisposable
	{
		private readonly IProducer<string, string> _producer;

		// Принимаем IOptions<KafkaSettings> для DI
		public ConfluentKafkaProducer(IOptions<KafkaSettings> options)
			: this(options?.Value ?? throw new ArgumentNullException(nameof(options)))
		{
		}

		// Старый конструктор для явной передачи настроек
		public ConfluentKafkaProducer(KafkaSettings settings)
		{
			if (settings == null)
				throw new ArgumentNullException(nameof(settings));

			// Настройки только для PLAINTEXT
			var cfg = new ProducerConfig
			{
				BootstrapServers = settings.BootstrapServers,
				Acks = Acks.All,
				EnableIdempotence = settings?.Options?.EnableIdempotence ?? false,
				SecurityProtocol = SecurityProtocol.Plaintext // Только PLAINTEXT
			};

			_producer = new ProducerBuilder<string, string>(cfg).Build();
		}

		public async Task ProduceAsync(string topic, string? key, string payload, CancellationToken ct = default)
		{
			var msg = new Message<string, string> { Key = key, Value = payload };
			await _producer.ProduceAsync(topic, msg, ct);
		}

		public void Dispose() => _producer?.Dispose();
	}
}
