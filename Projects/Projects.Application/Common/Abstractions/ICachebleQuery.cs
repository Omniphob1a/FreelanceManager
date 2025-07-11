using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projects.Application.Common.Abstractions
{
	public interface ICacheableQuery
	{
		string CacheKey { get; }
		TimeSpan Ttl { get; }
	}
}
