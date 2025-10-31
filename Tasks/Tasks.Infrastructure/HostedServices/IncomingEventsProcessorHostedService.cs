using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tasks.Application.Events;
using Tasks.Application.Interfaces;
using Tasks.Persistence.Data;

namespace Tasks.Infrastructure.HostedServices
{
	public class IncomingEventsProcessorHostedService : BackgroundService
	{
		private readonly IServiceProvider _sp;
		private readonly ILogger<IncomingEventsProcessorHostedService> _logger;
		private const int BatchSize = 20;

		public IncomingEventsProcessorHostedService(IServiceProvider sp, ILogger<IncomingEventsProcessorHostedService> logger)
		{
			_sp = sp;
			_logger = logger;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			while (!stoppingToken.IsCancellationRequested)
			{
				try
				{
					using var scope = _sp.CreateScope();
					var incomingRepo = scope.ServiceProvider.GetRequiredService<IIncomingEventRepository>();
					var processors = scope.ServiceProvider.GetServices<IIncomingEventProcessor>().ToList();
					var db = scope.ServiceProvider.GetRequiredService<ProjectTasksDbContext>();

					var pending = await incomingRepo.GetPendingAsync(BatchSize, stoppingToken);
					if (!pending.Any())
					{
						await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
						continue;
					}

					foreach (var ev in pending)
					{
						try
						{
							var handler = processors.FirstOrDefault(p => p.SupportedEventTypes.Contains(ev.EventType));
							if (handler == null)
							{
								_logger.LogWarning("No handler for {EventType}", ev.EventType);
								var next = DateTimeOffset.UtcNow.AddMinutes(5);
								await incomingRepo.IncrementRetryAsync(ev.Id, $"NoHandler:{ev.EventType}", next, stoppingToken);
								continue;
							}

							await handler.HandleAsync(ev, stoppingToken);
						}
						catch (Exception ex)
						{
							_logger.LogError(ex, "Processing failed for incoming {Id}", ev.Id);
							var next = DateTimeOffset.UtcNow.AddSeconds(Math.Pow(2, Math.Min(ev.RetryCount, 6)));
							await incomingRepo.IncrementRetryAsync(ev.Id, ex.Message, next, stoppingToken);
						}
					}
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "Processor loop failed");
					await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
				}
			}
		}
	}
}
