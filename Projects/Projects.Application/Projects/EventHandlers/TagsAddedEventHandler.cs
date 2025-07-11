using MediatR;
using Microsoft.Extensions.Logging;
using Projects.Application.Common.Abstractions;
using Projects.Application.Common.Cache;
using Projects.Application.Common.Notifications;
using Projects.Domain.Events;

namespace Projects.Application.Projects.EventHandlers;

public class TagsAddedEventHandler
	: INotificationHandler<DomainEventNotification<TagsAddedDomainEvent>>
{
	private readonly ICacheService _cache;
	private readonly ILogger<TagsAddedEventHandler> _logger;

	public TagsAddedEventHandler(
		ICacheService cache,
		ILogger<TagsAddedEventHandler> logger)
	{
		_cache = cache;
		_logger = logger;
	}

	public async Task Handle(DomainEventNotification<TagsAddedDomainEvent> notification, CancellationToken cancellationToken)
	{
		var e = notification.DomainEvent;

		_logger.LogInformation("Tag added to project {ProjectId}: {Tag}", e.ProjectId, e.Tag.Value);

		await _cache.RemoveAsync(CacheKeys.Project(e.ProjectId), cancellationToken);
	}
}
