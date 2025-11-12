using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projects.Application.Interfaces
{
	public interface IIncomingEventStore
	{
		Task SaveAsync(string topic, string? key, string? payload, CancellationToken ct);
	}
}
