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
	public class TagsDeletedEventHandler
	: INotificationHandler<DomainEventNotification<TagsDeletedDomainEvent>>
	{
		private readonly ICacheService _cache;
		private readonly ILogger<TagsDeletedEventHandler> _logger;

		public TagsDeletedEventHandler(
			ICacheService cache,
			ILogger<TagsDeletedEventHandler> logger)
		{
			_cache = cache;
			_logger = logger;
		}

		public async Task Handle(DomainEventNotification<TagsDeletedDomainEvent> notification, CancellationToken cancellationToken)
		{
			var e = notification.DomainEvent;

			_logger.LogInformation("Tag Deleted from project {ProjectId}: {Tag}", e.ProjectId, e.Tag.Value);

			await _cache.RemoveAsync(CacheKeys.Project(e.ProjectId), cancellationToken);
		}
	}
}
