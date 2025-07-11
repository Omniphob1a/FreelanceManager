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
	public class AttachmentAddedEventHandler : INotificationHandler<DomainEventNotification<AttachmentAddedDomainEvent>>
	{
		private readonly ICacheService _cache;
		private readonly ILogger<AttachmentAddedEventHandler> _logger;

		public AttachmentAddedEventHandler(
			ICacheService cache,
			ILogger<AttachmentAddedEventHandler> logger)
		{
			_cache = cache;
			_logger = logger;
		}

		public async Task Handle(DomainEventNotification<AttachmentAddedDomainEvent> notification, CancellationToken cancellationToken)
		{
			var e = notification.DomainEvent;
			_logger.LogInformation("Invalidating project list cache due to added attachment in project {ProjectId}", e.ProjectId);

			await _cache.RemoveAsync(CacheKeys.Project(e.ProjectId), cancellationToken);
		}
	}
}
