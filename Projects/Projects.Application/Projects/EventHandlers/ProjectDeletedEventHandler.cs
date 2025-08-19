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
	public class ProjectDeletedEventHandler : INotificationHandler<DomainEventNotification<ProjectDeletedDomainEvent>>
	{
		private readonly ILogger<ProjectDeletedEventHandler> _logger;
		private readonly ICacheService _cache;

		public ProjectDeletedEventHandler(ILogger<ProjectDeletedEventHandler> logger, ICacheService cache)
		{
			_logger = logger;
			_cache = cache;
		}

		public async Task Handle(DomainEventNotification<ProjectDeletedDomainEvent> notification, CancellationToken cancellationToken)
		{
			var e = notification.DomainEvent;

			_logger.LogInformation("Project deleted {ProjectId}", e.ProjectId);

			await _cache.RemoveAsync(CacheKeys.Project(e.ProjectId), cancellationToken);
			await _cache.RemoveByPrefixAsync(CacheKeys.FilteredProjectListPrefix, cancellationToken);
		}
	}
}
