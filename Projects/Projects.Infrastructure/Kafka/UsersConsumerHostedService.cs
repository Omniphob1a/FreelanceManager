using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Projects.Infrastructure.Kafka;
using Projects.Persistence.Data;
using Projects.Persistence.Models.ReadModels;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Users.Infrastructure.Kafka
{
	public class UsersConsumerHostedService : BackgroundService
	{
		private readonly ILogger<UsersConsumerHostedService> _logger;
		private readonly IServiceProvider _sp;
		private readonly KafkaSettings _settings;

		private IConsumer<string, string?>? _consumer;

		private const string Topic = "users";
		private static readonly TimeSpan ConsumeTimeout = TimeSpan.FromMilliseconds(500);

		public UsersConsumerHostedService(
			ILogger<UsersConsumerHostedService> logger,
			IServiceProvider sp,
			KafkaSettings settings)
		{
			_logger = logger;
			_sp = sp;
			_settings = settings;
		}

		public override Task StartAsync(CancellationToken cancellationToken)
		{
			var cfg = new ConsumerConfig
			{
				BootstrapServers = _settings.BootstrapServers,
				GroupId = _settings.GroupId,
				AutoOffsetReset = AutoOffsetReset.Earliest,
				EnableAutoCommit = false,
				EnablePartitionEof = false
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
				_logger.LogInformation("Users consumer subscribed to {Topic}", Topic);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to subscribe to topic {Topic}.", Topic);
			}

			while (!stoppingToken.IsCancellationRequested)
			{
				try
				{
					var cr = _consumer.Consume(ConsumeTimeout);
					if (cr == null) continue;

					var key = cr.Message?.Key ?? "";

					using var scope = _sp.CreateScope();
					var db = scope.ServiceProvider.GetRequiredService<ProjectsDbContext>();

					Guid.TryParse(key, out var userId);

					if (cr.Message.Value is null)
					{
						var entity = await db.Set<UserReadModel>()
							.FindAsync(new object[] { userId }, stoppingToken);

						if (entity != null) db.Remove(entity);

						await db.SaveChangesAsync(stoppingToken);
						SafeCommit(cr);
						continue;
					}

					var user = JsonSerializer.Deserialize<UserReadModel>(
						cr.Message.Value,
						new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

					if (user is null)
					{
						_logger.LogWarning("Failed to deserialize user: {Payload}", cr.Message.Value);
						SafeCommit(cr);
						continue;
					}

					var set = db.Set<UserReadModel>();
					var existing = await set.FindAsync(new object[] { user.Id }, stoppingToken);

					if (existing is null)
					{
						set.Add(user);
					}
					else
					{
						existing.Login = user.Login;
						existing.Name = user.Name;
						existing.Gender = user.Gender;
						existing.Birthday = user.Birthday;
					}

					await db.SaveChangesAsync(stoppingToken);
					SafeCommit(cr);
				}
				catch (ConsumeException ex)
				{
					_logger.LogError(ex,
						"Kafka consume error; code={Code}, reason={Reason}",
						ex.Error.Code, ex.Error.Reason);
					await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
				}
				catch (OperationCanceledException) { }
				catch (Exception ex)
				{
					_logger.LogError(ex, "Users consumer loop error");
					try { await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken); } catch { }
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
				_logger.LogWarning(e,
					"Commit failed; topic={Topic}, partition={Partition}, offset={Offset}",
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
