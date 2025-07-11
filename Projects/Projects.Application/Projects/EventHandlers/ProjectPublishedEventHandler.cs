using MediatR;
using Microsoft.Extensions.Logging;
using Projects.Application.Common.Abstractions;
using Projects.Application.Common.Cache;
using Projects.Application.Common.Notifications;
using Projects.Domain.Enums;
using Projects.Domain.Events;

namespace Projects.Application.Projects.EventHandlers;

public class ProjectPublishedEventHandler
	: INotificationHandler<DomainEventNotification<ProjectPublishedDomainEvent>>
{
	private readonly ICacheService _cache;
	private readonly ILogger<ProjectPublishedEventHandler> _logger;

	public ProjectPublishedEventHandler(
		ICacheService cache,
		ILogger<ProjectPublishedEventHandler> logger)
	{
		_cache = cache;
		_logger = logger;
	}

	public async Task Handle(DomainEventNotification<ProjectPublishedDomainEvent> notification, CancellationToken cancellationToken)
	{
		var e = notification.DomainEvent;

		_logger.LogInformation("Project {ProjectId} published. Invalidating related caches.", e.ProjectId);

		await _cache.RemoveAsync(CacheKeys.Project(e.ProjectId), cancellationToken);
	}
}
