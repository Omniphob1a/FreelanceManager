using Confluent.Kafka;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;
using Tasks.Infrastructure.Kafka;

namespace Tasks.Infrastructure.Kafka
{
	public class ConfluentKafkaProducer : IKafkaProducer, IDisposable
	{
		private readonly IProducer<string, string> _producer;

		public ConfluentKafkaProducer(IOptions<KafkaSettings> options)
			: this(options?.Value ?? throw new ArgumentNullException(nameof(options)))
		{
		}

		public ConfluentKafkaProducer(KafkaSettings settings)
		{
			if (settings == null)
				throw new ArgumentNullException(nameof(settings));

			// безопасный парсинг SecurityProtocol
			SecurityProtocol securityProtocol = SecurityProtocol.Plaintext;
			if (!string.IsNullOrWhiteSpace(settings.SecurityProtocol) &&
				Enum.TryParse<SecurityProtocol>(settings.SecurityProtocol, ignoreCase: true, out var sp))
			{
				securityProtocol = sp;
			}

			// безопасный парсинг SaslMechanism
			SaslMechanism saslMechanism = SaslMechanism.Plain;
			if (!string.IsNullOrWhiteSpace(settings.SaslMechanism) &&
				Enum.TryParse<SaslMechanism>(settings.SaslMechanism, ignoreCase: true, out var sm))
			{
				saslMechanism = sm;
			}

			var cfg = new ProducerConfig
			{
				BootstrapServers = settings.BootstrapServers,
				Acks = Acks.All,
				EnableIdempotence = settings?.Options?.EnableIdempotence ?? false,
				SecurityProtocol = securityProtocol,
				SaslMechanism = saslMechanism,
				SaslUsername = settings.SaslUsername,
				SaslPassword = settings.SaslPassword
			};

			// При отсутствии BootstrapServers можно не строить продьюсер — но здесь мы допускаем пустую строку и дастся ошибка при попытке использовать.
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
