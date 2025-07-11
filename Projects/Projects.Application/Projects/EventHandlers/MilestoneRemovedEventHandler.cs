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
	public class MilestoneRemovedEventHandler
	: INotificationHandler<DomainEventNotification<MilestoneRemovedDomainEvent>>
	{
		private readonly ICacheService _cache;
		private readonly ILogger<MilestoneRemovedEventHandler> _logger;

		public MilestoneRemovedEventHandler(ICacheService cache, ILogger<MilestoneRemovedEventHandler> logger)
		{
			_cache = cache;
			_logger = logger;
		}

		public async Task Handle(DomainEventNotification<MilestoneRemovedDomainEvent> notification, CancellationToken cancellationToken)
		{
			var e = notification.DomainEvent;

			_logger.LogInformation("Milestone removed from project {ProjectId}", e.ProjectId);

			await _cache.RemoveAsync(CacheKeys.Project(e.ProjectId), cancellationToken);
		}
	}


}
