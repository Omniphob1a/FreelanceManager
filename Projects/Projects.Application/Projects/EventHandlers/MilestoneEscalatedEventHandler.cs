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
	public class MilestoneEscalatedEventHandler : INotificationHandler<DomainEventNotification<MilestoneEscalatedDomainEvent>>
	{
		public ICacheService _cache;
		public ILogger<MilestoneEscalatedEventHandler> _logger;

		public MilestoneEscalatedEventHandler(ICacheService cache, ILogger<MilestoneEscalatedEventHandler> logger)
		{
			_cache = cache;
			_logger = logger;
		}

		public async Task Handle(DomainEventNotification<MilestoneEscalatedDomainEvent> notification, CancellationToken cancellationToken)
		{
			var e = notification.DomainEvent;

			_logger.LogInformation("Milestone escalated in project {ProjectId}", notification.DomainEvent.ProjectId);

			await _cache.RemoveAsync(CacheKeys.Project(e.ProjectId), cancellationToken);
		}
	}
}
