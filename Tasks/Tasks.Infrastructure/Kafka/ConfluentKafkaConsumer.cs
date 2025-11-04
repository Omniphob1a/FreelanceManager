using Confluent.Kafka;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tasks.Infrastructure.Kafka
{
	public class ConfluentKafkaConsumer : IKafkaConsumer
	{
		private readonly IConsumer<string, string> _consumer;

		public ConfluentKafkaConsumer(KafkaSettings settings, string groupId)
		{
			var cfg = new ConsumerConfig
			{
				BootstrapServers = settings.BootstrapServers,
				GroupId = groupId,
				AutoOffsetReset = AutoOffsetReset.Earliest,
				EnableAutoCommit = true,
				SecurityProtocol = Enum.Parse<SecurityProtocol>(settings.SecurityProtocol),
				SaslMechanism = Enum.Parse<SaslMechanism>(settings.SaslMechanism),
				SaslUsername = settings.SaslUsername,
				SaslPassword = settings.SaslPassword
			};

			_consumer = new ConsumerBuilder<string, string>(cfg).Build();
		}

		public async Task ConsumeAsync(string topic, Func<string?, string, Task> handler, CancellationToken ct = default)
		{
			_consumer.Subscribe(topic);

			try
			{
				while (!ct.IsCancellationRequested)
				{
					var cr = _consumer.Consume(ct);
					if (cr != null)
					{
						await handler(cr.Message.Key, cr.Message.Value);
					}
				}
			}
			catch (OperationCanceledException)
			{
				// игнорируем, т.к. это нормальное завершение по токену
			}
		}

		public void Dispose() => _consumer?.Close();
	}
}
