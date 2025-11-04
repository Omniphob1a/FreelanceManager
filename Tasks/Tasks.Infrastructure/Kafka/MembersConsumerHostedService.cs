using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Tasks.Infrastructure.DTOs;
using Tasks.Persistence.Data;
using Tasks.Persistence.Models.ReadModels;

namespace Tasks.Infrastructure.Kafka
{
	public sealed class MembersConsumerHostedService : BackgroundService
	{
		private readonly ILogger<MembersConsumerHostedService> _logger;
		private readonly IServiceProvider _sp;
		private readonly KafkaSettings _settings;

		private IConsumer<string, string?>? _consumer;

		private const string Topic = "members";
		private static readonly TimeSpan ConsumeTimeout = TimeSpan.FromMilliseconds(500);

		public MembersConsumerHostedService(
			ILogger<MembersConsumerHostedService> logger,
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
				_logger.LogInformation("Members consumer subscribed to {Topic}", Topic);
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

					if (cr == null)
						continue;

					var key = cr.Message?.Key ?? "";
					var parts = key.Split(':', 2);

					if (parts.Length != 2 || !Guid.TryParse(parts[0], out var projectId) || !Guid.TryParse(parts[1], out var userId))
					{
						_logger.LogWarning("Bad key format: {Key}", key);
						SafeCommit(cr);
						continue;
					}

					using var scope = _sp.CreateScope();
					var db = scope.ServiceProvider.GetRequiredService<ProjectTasksDbContext>();

					if (cr.Message.Value is null)
					{
						var entity = await db.Set<MemberReadModel>()
							.FindAsync(new object[] { projectId, userId }, stoppingToken);
						if (entity != null)
							db.Remove(entity);

						await db.SaveChangesAsync(stoppingToken);
						SafeCommit(cr);
						continue;
					}

					var member = JsonSerializer.Deserialize<ProjectMemberDto>(
						cr.Message.Value,
						new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

					if (member is null)
					{
						_logger.LogWarning("Failed to deserialize member: {Payload}", cr.Message.Value);
						SafeCommit(cr);
						continue;
					}

					var set = db.Set<MemberReadModel>();
					var existing = await set.FindAsync(new object[] { member.ProjectId, member.UserId }, stoppingToken);

					if (existing is null)
					{
						set.Add(new MemberReadModel
						{
							Id = member.Id,
							ProjectId = member.ProjectId,
							UserId = member.UserId,
							Role = member.Role,
							AddedAt = member.AddedAt
						});
					}
					else
					{
						existing.Role = member.Role;
					}

					await db.SaveChangesAsync(stoppingToken);
					SafeCommit(cr);
				}
				catch (ConsumeException ex)
				{
					_logger.LogError(ex, "Kafka consume error; code={Code}, reason={Reason}", ex.Error.Code, ex.Error.Reason);
					await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
				}
				catch (OperationCanceledException)
				{
					// graceful shutdown
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "Members consumer loop error");
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
