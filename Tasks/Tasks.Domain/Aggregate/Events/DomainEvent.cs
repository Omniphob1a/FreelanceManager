using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tasks.Domain.Interfaces;

namespace Tasks.Domain.Aggregate.Events
{
	public abstract record DomainEvent(Guid AggregateId) : IDomainEvent
	{
		public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
	}
}
