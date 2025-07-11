using Projects.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projects.Application.Interfaces
{
	public interface IDomainEventDispatcher
	{
		Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken ct = default);
	}
}
