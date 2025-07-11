using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projects.Application.Common.Pagination
{
	public class PaginatedResult<T>
	{
		public IReadOnlyList<T> Items { get; }
		public PaginationInfo Pagination { get; }

		public PaginatedResult(IEnumerable<T> items, int totalItems, int actualPage, int itemsPerPage)
		{
			Items = items.ToList();
			Pagination = new PaginationInfo(totalItems, itemsPerPage, actualPage);
		}
	}
}
