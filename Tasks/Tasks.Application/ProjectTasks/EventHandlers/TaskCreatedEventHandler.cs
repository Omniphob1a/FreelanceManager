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
	public class TaskCreatedEventHandler : INotificationHandler<DomainEventNotification<TaskCreatedDomainEvent>>
	{
		private readonly ICacheService _cache;
		private readonly ILogger<TaskCreatedEventHandler> _logger;

		public TaskCreatedEventHandler(ICacheService cache, ILogger<TaskCreatedEventHandler> logger)
		{
			_cache = cache;
			_logger = logger;
		}

		public async Task Handle(DomainEventNotification<TaskCreatedDomainEvent> notification, CancellationToken cancellationToken)
		{
			var e = notification.DomainEvent;
			_logger.LogInformation("Invalidating caches due to created task {TaskId} in project {ProjectId}", e.TaskId, e.ProjectId);

			await _cache.RemoveAsync(CacheKeys.Task(e.TaskId), cancellationToken);
			await _cache.RemoveByPrefixAsync(CacheKeys.FilteredTaskListPrefix, cancellationToken);
		}
	}
}
