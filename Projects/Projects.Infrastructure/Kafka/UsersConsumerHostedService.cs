using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Projects.Application.Interfaces;
using Projects.Infrastructure.Kafka;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Projects.Infrastructure.Kafka
{
	public sealed class UsersConsumerHostedService : BackgroundService
	{
		private readonly ILogger<UsersConsumerHostedService> _logger;
		private readonly IServiceProvider _sp;
		private readonly KafkaSettings _settings;

		private IConsumer<string, string?>? _consumer;
		private string? _effectiveGroup;

		private const string Topic = "users";
		private static readonly TimeSpan ConsumeTimeout = TimeSpan.FromMilliseconds(500);

		public UsersConsumerHostedService(
			ILogger<UsersConsumerHostedService> logger,
			IServiceProvider sp,
			KafkaSettings settings)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_sp = sp ?? throw new ArgumentNullException(nameof(sp));
			_settings = settings ?? new KafkaSettings();
		}

		public override Task StartAsync(CancellationToken cancellationToken)
		{
			_effectiveGroup = string.IsNullOrWhiteSpace(_settings.GroupId)
				? $"tasks-{Topic}-{Guid.NewGuid():n}".Substring(0, 20)
				: _settings.GroupId;

			_logger.LogInformation("Users consumer group = {Group} (consumer will be created in ExecuteAsync)", _effectiveGroup);
			return base.StartAsync(cancellationToken);
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			await Task.Yield();
			try { await Task.Delay(TimeSpan.FromMilliseconds(200), stoppingToken); } catch { }

			var cfg = BuildConsumerConfig();

			var attempt = 0;
			while (!stoppingToken.IsCancellationRequested)
			{
				try
				{
					_consumer = new ConsumerBuilder<string, string?>(cfg)
						.SetErrorHandler((_, e) => _logger.LogError("Kafka consumer error: {Reason} (code: {Code})", e.Reason, e.Code))
						.SetPartitionsAssignedHandler((c, partitions) => _logger.LogInformation("Partitions assigned: {@Partitions}", partitions))
						.SetPartitionsRevokedHandler((c, partitions) => _logger.LogInformation("Partitions revoked: {@Partitions}", partitions))
						.SetLogHandler((c, logMessage) => _logger.LogDebug("librdkafka {Level}/{Name}: {Message}", logMessage.Level, logMessage.Name, logMessage.Message))
						.Build();

					_logger.LogInformation("Users consumer created; group = {Group}", _effectiveGroup);
					break;
				}
				catch (Exception ex)
				{
					attempt++;
					_logger.LogError(ex, "Failed to create Kafka consumer — retrying in 5s (attempt {Attempt})", attempt);
					try { await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken); } catch { }
				}
			}

			if (_consumer == null)
			{
				_logger.LogError("Consumer was not created; ExecuteAsync exiting.");
				return;
			}

			try
			{
				try
				{
					_consumer.Subscribe(Topic);
					_logger.LogInformation("Users consumer subscribed to {Topic}", Topic);
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "Failed to subscribe to topic {Topic}. Subscription will be retried inside loop.", Topic);
				}

				while (!stoppingToken.IsCancellationRequested)
				{
					if (_consumer.Subscription == null || _consumer.Subscription.Count == 0)
					{
						try { _consumer.Subscribe(Topic); }
						catch (Exception ex) { _logger.LogWarning(ex, "Retry subscribe failed; backing off 2s"); try { await Task.Delay(2000, stoppingToken); } catch { } continue; }
					}

					try
					{
						ConsumeResult<string, string?>? cr = null;
						try { dynamic dyn = _consumer; cr = dyn.Consume(stoppingToken); } catch { cr = _consumer.Consume(ConsumeTimeout); }

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
							_logger.LogError(ex, "Failed to save incoming user event; will wait and continue");
							try { await Task.Delay(2000, stoppingToken); } catch { }
						}
					}
					catch (ConsumeException ex)
					{
						_logger.LogError(ex, "Kafka consume error; code={Code}, reason={Reason}", ex.Error?.Code, ex.Error?.Reason);
						try { await Task.Delay(2000, stoppingToken); } catch { }
					}
					catch (OperationCanceledException) { break; }
					catch (Exception ex)
					{
						_logger.LogError(ex, "Users consumer loop error");
						try { await Task.Delay(2000, stoppingToken); } catch { }
					}
				}
			}
			finally
			{
				if (_consumer != null)
				{
					try { _logger.LogInformation("Closing Kafka consumer..."); _consumer.Close(); }
					catch (Exception ex) { _logger.LogWarning(ex, "Error while closing Kafka consumer"); }
					finally { _consumer.Dispose(); _consumer = null; }
				}
			}
		}

		private ConsumerConfig BuildConsumerConfig()
		{
			var effectiveGroup = _effectiveGroup ?? (_settings.GroupId ?? $"tasks-{Topic}-{Guid.NewGuid():n}".Substring(0, 20));

			var cfg = new ConsumerConfig
			{
				BootstrapServers = _settings.BootstrapServers,
				GroupId = effectiveGroup,
				AutoOffsetReset = AutoOffsetReset.Earliest,
				EnableAutoCommit = false,
				EnablePartitionEof = false,
				SessionTimeoutMs = 30000,
				HeartbeatIntervalMs = 10000,
				ReconnectBackoffMs = 1000,
				ReconnectBackoffMaxMs = 10000,
				StatisticsIntervalMs = 60000,
				MaxPollIntervalMs = 300000
			};

			if (!string.IsNullOrWhiteSpace(_settings.SecurityProtocol) &&
				Enum.TryParse<SecurityProtocol>(_settings.SecurityProtocol, true, out var secProto))
				cfg.SecurityProtocol = secProto;

			if (!string.IsNullOrWhiteSpace(_settings.SaslMechanism) &&
				Enum.TryParse<SaslMechanism>(_settings.SaslMechanism, true, out var saslMech))
				cfg.SaslMechanism = saslMech;

			if (!string.IsNullOrWhiteSpace(_settings.SaslUsername)) cfg.SaslUsername = _settings.SaslUsername;
			if (!string.IsNullOrWhiteSpace(_settings.SaslPassword)) cfg.SaslPassword = _settings.SaslPassword;

			try { if (!string.IsNullOrWhiteSpace(_settings.SslCaLocation)) cfg.Set("ssl.ca.location", _settings.SslCaLocation); } catch { }
			try { cfg.Set("socket.keepalive.enable", "true"); } catch { }
			try { cfg.Set("request.timeout.ms", "60000"); } catch { }
			try { cfg.Set("enable.auto.offset.store", "false"); } catch { }

			return cfg;
		}

		private void SafeCommit(ConsumeResult<string, string?> cr)
		{
			try { _consumer?.Commit(cr); }
			catch (Exception e)
			{
				_logger.LogWarning(e, "Commit failed; topic={Topic}, partition={Partition}, offset={Offset}", cr.Topic, cr.Partition, cr.Offset);
			}
		}

		public override void Dispose()
		{
			if (_consumer != null)
			{
				try { _consumer.Close(); }
				catch (Exception ex) { _logger.LogWarning(ex, "Error while closing Kafka consumer"); }
				finally { _consumer.Dispose(); _consumer = null; }
			}
			base.Dispose();
		}
	}
}
