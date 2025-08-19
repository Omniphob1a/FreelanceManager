using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projects.Application.Common.Abstractions
{
	public interface ICacheableQuery
	{
		bool BypassCache { get; }
		string CacheKey { get; }
		int SlidingExpirationInMinutes { get; }
		int AbsoluteExpirationInMinutes { get; }
	}
}
