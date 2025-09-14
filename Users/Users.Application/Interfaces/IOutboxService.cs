using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Users.Application.Interfaces
{
	public interface IOutboxService
	{
		Task Add(object message, string topic, string? key = null, CancellationToken ct = default);
		Task AddTombstone(string topic, string key, CancellationToken ct = default);

	}
}
