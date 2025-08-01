﻿using Projects.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projects.Domain.Events
{
	public abstract record DomainEvent(Guid AggregateId) : IDomainEvent
	{
		public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
	}
}
