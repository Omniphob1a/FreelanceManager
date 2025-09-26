using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tasks.Application.Common.Pagination;

namespace Tasks.Application.Interfaces
{
	public interface ICacheableQuery
	{
		bool BypassCache { get; }
		string CacheKey { get; }
		int SlidingExpirationInMinutes { get; }
		int AbsoluteExpirationInMinutes { get; }
		PaginationInfo? PaginationInfo => null;
	}
}
