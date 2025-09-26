using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using Tasks.Application.Common.Cache;
using Tasks.Application.Common.Notifications;
using Tasks.Application.Interfaces;
using Tasks.Domain.Aggregate.Events;

namespace Tasks.Application.EventHandlers
{
	public class TaskDeletedEventHandler : INotificationHandler<DomainEventNotification<TaskDeletedDomainEvent>>
	{
		private readonly ICacheService _cache;
		private readonly ILogger<TaskDeletedEventHandler> _logger;

		public TaskDeletedEventHandler(ICacheService cache, ILogger<TaskDeletedEventHandler> logger)
		{
			_cache = cache;
			_logger = logger;
		}

		public async Task Handle(DomainEventNotification<TaskDeletedDomainEvent> notification, CancellationToken cancellationToken)
		{
			var e = notification.DomainEvent;
			_logger.LogInformation("Invalidating caches due to deleted task {TaskId}", e.TaskId);

			await _cache.RemoveAsync(CacheKeys.Task(e.TaskId), cancellationToken);
			await _cache.RemoveByPrefixAsync(CacheKeys.FilteredTaskListPrefix, cancellationToken);
		}
	}
}
