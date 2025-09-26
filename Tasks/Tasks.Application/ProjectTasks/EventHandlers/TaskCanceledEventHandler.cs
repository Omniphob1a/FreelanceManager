using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tasks.Application.Common.Cache;
using Tasks.Application.Common.Notifications;
using Tasks.Application.Interfaces;
using Tasks.Domain.Aggregate.Events;

namespace Tasks.Application.ProjectTasks.EventHandlers
{
	public class TaskCanceledEventHandler : INotificationHandler<DomainEventNotification<TaskCanceledDomainEvent>>
	{
		private readonly ICacheService _cache;
		private readonly ILogger<TaskCanceledEventHandler> _logger;

		public TaskCanceledEventHandler(
			ICacheService cache,
			ILogger<TaskCanceledEventHandler> logger)
		{
			_cache = cache;
			_logger = logger;
		}

		public async Task Handle(DomainEventNotification<TaskCanceledDomainEvent> notification, CancellationToken cancellationToken)
		{
			var e = notification.DomainEvent;
			_logger.LogInformation("Invalidating task cache because task {TaskId}", e.TaskId);

			await _cache.RemoveAsync(CacheKeys.Task(e.TaskId), cancellationToken);
			await _cache.RemoveByPrefixAsync(CacheKeys.FilteredTaskListPrefix, cancellationToken);

		}
	}
}
