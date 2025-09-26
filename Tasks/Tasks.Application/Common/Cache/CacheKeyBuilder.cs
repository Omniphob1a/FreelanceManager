using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tasks.Application.Common.Filters;
using Tasks.Application.Common.Pagination;

namespace Tasks.Application.Common.Cache
{
	public static class CacheKeyBuilder
	{
		public static string BuildTaskListKey(TaskFilter filter, PaginationInfo pagination)
		{
			var filterKey = filter?.ToCacheKey() ?? "nofilter|";
			var page = pagination?.ActualPage ?? 1;
			var pageSize = pagination?.ItemsPerPage ?? 10;

			return $"task:list:filtered:{filterKey}page={page}|pageSize={pageSize}";
		}
	}
}
