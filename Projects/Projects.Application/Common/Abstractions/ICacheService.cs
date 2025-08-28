using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projects.Application.Common.Abstractions
{
	public interface ICacheService
	{
		Task<T?> GetAsync<T>(string key, CancellationToken ct);
		Task SetAsync<T>(string key, T value, TimeSpan ttl, CancellationToken ct);
		Task RemoveAsync(string key, CancellationToken ct);
		Task RemoveByPrefixAsync(string prefix, CancellationToken ct);
	}
}
