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
	public class MilestoneAddedEventHandler
	: INotificationHandler<DomainEventNotification<MilestoneAddedDomainEvent>>
	{
		private readonly ICacheService _cache;
		private readonly ILogger<MilestoneAddedEventHandler> _logger;

		public MilestoneAddedEventHandler(ICacheService cache, ILogger<MilestoneAddedEventHandler> logger)
		{
			_cache = cache;
			_logger = logger;
		}

		public async Task Handle(DomainEventNotification<MilestoneAddedDomainEvent> notification, CancellationToken cancellationToken)
		{
			var e = notification.DomainEvent;

			_logger.LogInformation("Milestone added to project {ProjectId}", e.ProjectId);

			await _cache.RemoveAsync(CacheKeys.Project(e.ProjectId), cancellationToken);
		}
	}

}
