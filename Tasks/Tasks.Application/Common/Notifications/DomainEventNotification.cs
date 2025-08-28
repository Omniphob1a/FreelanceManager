using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tasks.Domain.Interfaces;

namespace Tasks.Application.Common.Notifications;

public class DomainEventNotification<TDomainEvent> : INotification
where TDomainEvent : IDomainEvent
{
	public TDomainEvent DomainEvent { get; }

	public DomainEventNotification(TDomainEvent domainEvent)
		=> DomainEvent = domainEvent;
}
