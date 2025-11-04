using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Tasks.Application.Events;
using Tasks.Application.Interfaces;
using Tasks.Persistence.Data;
using Tasks.Persistence.Models.ReadModels;

namespace Tasks.Infrastructure.Kafka
{
	public sealed class ProjectsConsumerHostedService : BackgroundService
	{
		private readonly ILogger<ProjectsConsumerHostedService> _logger;
		private readonly IServiceProvider _sp;
		private readonly KafkaSettings _settings;

		private IConsumer<string, string?>? _consumer;

		private const string Topic = "projects";
		private static readonly TimeSpan ConsumeTimeout = TimeSpan.FromMilliseconds(500);

		public ProjectsConsumerHostedService(
			ILogger<ProjectsConsumerHostedService> logger,
			IServiceProvider sp,
			KafkaSettings settings)
		{
			_logger = logger;
			_sp = sp;
			_settings = settings;
		}

		public override Task StartAsync(CancellationToken cancellationToken)
		{
			var effectiveGroup = string.IsNullOrWhiteSpace(_settings.GroupId)
				? $"tasks-{Topic}-{Guid.NewGuid():n}".Substring(0, 20)
				: _settings.GroupId;

			var cfg = new ConsumerConfig
			{
				BootstrapServers = _settings.BootstrapServers,
				GroupId = _settings.GroupId,
				AutoOffsetReset = AutoOffsetReset.Earliest,
				EnableAutoCommit = false,
				EnablePartitionEof = false,
				SecurityProtocol = Enum.Parse<SecurityProtocol>(_settings.SecurityProtocol),
				SaslMechanism = Enum.Parse<SaslMechanism>(_settings.SaslMechanism),
				SaslUsername = _settings.SaslUsername,
				SaslPassword = _settings.SaslPassword
			};

			_consumer = new ConsumerBuilder<string, string?>(cfg)
				.SetErrorHandler((_, e) =>
					_logger.LogError("Kafka consumer error: {Reason} (code: {Code})", e.Reason, e.Code))
				.SetPartitionsAssignedHandler((c, partitions) =>
					_logger.LogInformation("Partitions assigned: {@Partitions}", partitions))
				.SetPartitionsRevokedHandler((c, partitions) =>
					_logger.LogInformation("Partitions revoked: {@Partitions}", partitions))
				.Build();

			return base.StartAsync(cancellationToken);
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			if (_consumer == null)
			{
				_logger.LogError("Consumer was not initialized");
				return;
			}

			try
			{
				_consumer.Subscribe(Topic);
				_logger.LogInformation("Projects consumer subscribed to {Topic}", Topic);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to subscribe to topic {Topic}.", Topic);
			}

			while (!stoppingToken.IsCancellationRequested)
			{
				var cr = _consumer.Consume(ConsumeTimeout);
				if (cr == null) continue;

				using var scope = _sp.CreateScope();
				var store = scope.ServiceProvider.GetRequiredService<IIncomingEventStore>();

				try
				{
					await store.SaveAsync(cr.Topic, cr.Message?.Key, cr.Message?.Value, stoppingToken);
					SafeCommit(cr);
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "Failed to save incoming project event");
					await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
				}
			}
		}

		private void SafeCommit(ConsumeResult<string, string?> cr)
		{
			try
			{
				_consumer?.Commit(cr);
			}
			catch (Exception e)
			{
				_logger.LogWarning(e, "Commit failed; topic={Topic}, partition={Partition}, offset={Offset}",
					cr.Topic, cr.Partition, cr.Offset);
			}
		}

		public override void Dispose()
		{
			if (_consumer != null)
			{
				try { _consumer.Close(); }
				catch (Exception ex) { _logger.LogWarning(ex, "Error while closing Kafka consumer"); }
				finally { _consumer.Dispose(); }
			}
			base.Dispose();
		}
	}
}
