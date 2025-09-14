using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Users.Infrastructure.Data;
using Users.Infrastructure.Kafka;

namespace Users.Infrastructure.Outbox
{
	public class OutboxPublisherHostedService : BackgroundService
	{
		private readonly IServiceProvider _serviceProvider;
		private readonly IKafkaProducer _producer;
		private readonly ILogger<OutboxPublisherHostedService> _logger;
		private readonly int _batchSize = 50;
		private readonly TimeSpan _pollInterval = TimeSpan.FromSeconds(5);
		private readonly int _maxRetries = 5;

		public OutboxPublisherHostedService(
			IServiceProvider serviceProvider,
			IKafkaProducer producer,
			ILogger<OutboxPublisherHostedService> logger)
		{
			_serviceProvider = serviceProvider;
			_producer = producer;
			_logger = logger;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			_logger.LogInformation("OutboxPublisherHostedService started.");
			while (!stoppingToken.IsCancellationRequested)
			{
				try 
				{
					using var scope = _serviceProvider.CreateScope();
					var db = scope.ServiceProvider.GetRequiredService<UsersDbContext>();

					var batch = await db.OutboxMessages
						.Where(o => !o.Processed)
						.OrderBy(o => o.OccurredAt)
						.Take(_batchSize)
						.ToListAsync(stoppingToken);

					if (batch.Count == 0)
					{
						await Task.Delay(_pollInterval, stoppingToken);
						continue;
					}

					foreach (var msg in batch)
					{
						try
						{
							await _producer.ProduceAsync(msg.Topic, msg.Key, msg.Payload, stoppingToken);
							msg.Processed = true;
							msg.ProcessedAt = DateTime.UtcNow;
							msg.LastError = null;
							_logger.LogInformation("Outbox published: {Id} -> {Topic} [{Key}]", msg.Id, msg.Topic, msg.Key);
						}
						catch (Exception ex)
						{
							msg.RetryCount++;
							msg.LastError = ex.Message;
							_logger.LogWarning(ex, "Failed to publish outbox id={Id}, retry={Retry}", msg.Id, msg.RetryCount);

							if (msg.RetryCount >= _maxRetries)
							{            
								//помечаем processed чтобы не блокировать очередь
								msg.Processed = true;
								msg.ProcessedAt = DateTime.UtcNow;
								_logger.LogError("Outbox message {Id} failed {Retry} times — marking processed and skipping.", msg.Id, msg.RetryCount);
							}
						}
					}

					await db.SaveChangesAsync(stoppingToken);
				}
				catch (OperationCanceledException) { }
				catch (Exception ex)
				{
					_logger.LogError(ex, "OutboxPublisher error");
					await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
				}
			}
			_logger.LogInformation("OutboxPublisherHostedService stopping.");
		}
	}
}
