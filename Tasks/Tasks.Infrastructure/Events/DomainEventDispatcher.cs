using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tasks.Application.Common.Notifications;
using Tasks.Application.Interfaces;
using Tasks.Domain.Interfaces;

namespace Tasks.Infrastructure.Events
{
	public sealed class DomainEventDispatcher : IDomainEventDispatcher
	{
		private readonly IMediator _mediator;

		public DomainEventDispatcher(IMediator mediator) => _mediator = mediator;

		public async Task DispatchAsync(IEnumerable<IDomainEvent> events, CancellationToken ct = default)
		{
			foreach (var domainEvent in events)
			{
				var notificationType = typeof(DomainEventNotification<>).MakeGenericType(domainEvent.GetType());
				var notification = (INotification)Activator.CreateInstance(notificationType, domainEvent)!;

				await _mediator.Publish(notification, ct);
			}
		}
	}
}
