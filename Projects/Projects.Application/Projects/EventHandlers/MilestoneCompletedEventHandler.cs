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
	public class MilestoneCompletedEventHandler : INotificationHandler<DomainEventNotification<MilestoneCompletedDomainEvent>>
	{
		private readonly ICacheService _cache;
		private readonly ILogger<MilestoneCompletedEventHandler> _logger;

		public MilestoneCompletedEventHandler(ICacheService cache, ILogger<MilestoneCompletedEventHandler> logger)
		{
			_cache = cache;
			_logger = logger;
		}

		public async Task Handle(DomainEventNotification<MilestoneCompletedDomainEvent> notification, CancellationToken cancellationToken)
		{
			var e = notification.DomainEvent;

			_logger.LogInformation("Milestone completed for project {ProjectId}", notification.DomainEvent.ProjectId);

			await _cache.RemoveAsync(CacheKeys.Project(e.ProjectId), cancellationToken);
		}
	}
}
