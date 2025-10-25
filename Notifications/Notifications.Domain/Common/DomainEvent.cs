using Notifications.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Notifications.Domain.Common
{
	public abstract record DomainEvent(Guid AggregateId) : IDomainEvent
	{
		public Guid EventId { get; } = Guid.NewGuid();
		public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
		public virtual string EventType => GetType().FullName!;
		public virtual int Version => 1;
	}
}
