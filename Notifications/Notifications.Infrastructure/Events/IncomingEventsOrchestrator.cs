using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Notifications.Application.Events;
using Notifications.Application.Interfaces;
using Notifications.Persistence.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Notifications.Infrastructure.Events
{
	public class IncomingEventsOrchestrator : BackgroundService
	{
		private readonly IServiceProvider _sp;
		private readonly ILogger<IncomingEventsOrchestrator> _logger;
		private const int BatchSize = 20;

		public IncomingEventsOrchestrator(IServiceProvider sp, ILogger<IncomingEventsOrchestrator> logger)
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
					var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
					var incomingRepo = scope.ServiceProvider.GetRequiredService<IIncomingEventRepository>();
					var handlers = scope.ServiceProvider.GetServices<IEventHandler>().ToList();
					var db = scope.ServiceProvider.GetRequiredService<NotificationsDbContext>();

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
							var handler = handlers.FirstOrDefault(h => h.EventType.Equals(ev.EventType, StringComparison.OrdinalIgnoreCase));
							if (handler == null)
							{
								_logger.LogWarning("No handler for {EventType}", ev.EventType);
								var next = DateTimeOffset.UtcNow.AddMinutes(5);
								await incomingRepo.IncrementRetryAsync(ev.Id, $"NoHandler:{ev.EventType}", next, stoppingToken);
								continue;
							}
							await mediator.Send(handler.HandleAsync(ev, stoppingToken));
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
					_logger.LogError(ex, "Orchestrator loop failed");
					await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
				}
			}

		}
	}
}
