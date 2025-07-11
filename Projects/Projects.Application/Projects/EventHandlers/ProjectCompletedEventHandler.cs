using MediatR;
using Microsoft.Extensions.Logging;
using Projects.Application.Common.Abstractions;
using Projects.Application.Common.Cache;
using Projects.Application.Common.Notifications;
using Projects.Domain.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projects.Application.Projects.EventHandlers
{
	public class ProjectCompletedEventHandler
	: INotificationHandler<DomainEventNotification<ProjectCompletedDomainEvent>>
	{
		private readonly ICacheService _cache;
		private readonly ILogger<ProjectCompletedEventHandler> _logger;

		public ProjectCompletedEventHandler(ICacheService cache, ILogger<ProjectCompletedEventHandler> logger)
		{
			_cache = cache;
			_logger = logger;
		}

		public async Task Handle(DomainEventNotification<ProjectCompletedDomainEvent> notification, CancellationToken cancellationToken)
		{
			var e = notification.DomainEvent;

			_logger.LogInformation("Project completed {ProjectId}", e.ProjectId);

			await _cache.RemoveAsync(CacheKeys.Project(e.ProjectId), cancellationToken);
			await _cache.RemoveByPrefixAsync(CacheKeys.FilteredProjectListPrefix, cancellationToken);
		}
	}

}
