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
	public class TaskCompletedEventHandler : INotificationHandler<DomainEventNotification<TaskCompletedDomainEvent>>
	{
		private readonly ICacheService _cache;
		private readonly ILogger<TaskCompletedEventHandler> _logger;

		public TaskCompletedEventHandler(
			ICacheService cache,
			ILogger<TaskCompletedEventHandler> logger)
		{
			_cache = cache;
			_logger = logger;
		}

		public async Task Handle(DomainEventNotification<TaskCompletedDomainEvent> notification, CancellationToken cancellationToken)
		{
			var e = notification.DomainEvent;
			_logger.LogInformation("Invalidating task cache because task {TaskId} was completed", e.TaskId);

			await _cache.RemoveAsync(CacheKeys.Task(e.TaskId), cancellationToken);
			await _cache.RemoveByPrefixAsync(CacheKeys.FilteredTaskListPrefix, cancellationToken);

		}
	}
}
