using MediatR;
using Projects.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projects.Application.Common.Notifications;

public class DomainEventNotification<TDomainEvent> : INotification
where TDomainEvent : IDomainEvent
{
	public TDomainEvent DomainEvent { get; }

	public DomainEventNotification(TDomainEvent domainEvent)
		=> DomainEvent = domainEvent;
}
