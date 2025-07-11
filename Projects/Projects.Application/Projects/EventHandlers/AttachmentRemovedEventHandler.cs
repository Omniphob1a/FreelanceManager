using MediatR;
using Microsoft.Extensions.Logging;
using Projects.Application.Common.Abstractions;
using Projects.Application.Common.Cache;
using Projects.Application.Common.Notifications;
using Projects.Domain.Events;

namespace Projects.Application.Projects.EventHandlers;

public class AttachmentRemovedEventHandler
	: INotificationHandler<DomainEventNotification<AttachmentRemovedDomainEvent>>
{
	private readonly ICacheService _cache;
	private readonly ILogger<AttachmentRemovedEventHandler> _logger;

	public AttachmentRemovedEventHandler(
		ICacheService cache,
		ILogger<AttachmentRemovedEventHandler> logger)
	{
		_cache = cache;
		_logger = logger;
	}

	public async Task Handle(DomainEventNotification<AttachmentRemovedDomainEvent> notification, CancellationToken cancellationToken)
	{
		var e = notification.DomainEvent;

		_logger.LogInformation("Attachment removed from project {ProjectId}", e.ProjectId);

		await _cache.RemoveAsync(CacheKeys.Project(e.ProjectId), cancellationToken);
	}
}
