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
	public class ProjectCreatedEventHandler
	: INotificationHandler<DomainEventNotification<ProjectCreatedDomainEvent>>
	{
		private readonly ICacheService _cache;
		private readonly ILogger<ProjectCreatedEventHandler> _logger;

		public ProjectCreatedEventHandler(ICacheService cache, ILogger<ProjectCreatedEventHandler> logger)
		{
			_cache = cache;
			_logger = logger;
		}

		public async Task Handle(DomainEventNotification<ProjectCreatedDomainEvent> notification, CancellationToken cancellationToken)
		{
			var e = notification.DomainEvent;

			_logger.LogInformation("Project created {ProjectId}", e.ProjectId);

			await _cache.RemoveByPrefixAsync(CacheKeys.FilteredProjectListPrefix, cancellationToken);
		}
	}

}
