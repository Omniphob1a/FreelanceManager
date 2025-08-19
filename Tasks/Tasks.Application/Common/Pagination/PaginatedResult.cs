using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tasks.Application.Common.Pagination
{
	public class PaginatedResult<T>
	{
		public IReadOnlyList<T> Items { get; init; }
		public PaginationInfo Pagination {  get; init; }
		public PaginatedResult() { }

		public PaginatedResult(IEnumerable<T> items, PaginationInfo paginationInfo)
		{
			Items = items.ToList();
			Pagination = paginationInfo;
		}
	}
}
