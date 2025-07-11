using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projects.Application.Common.Pagination
{
	public class PaginationInfo
	{
		public int TotalItems { get; }
		public int ItemsPerPage { get; }
		public int ActualPage { get; }
		public int TotalPages => (int)Math.Ceiling((double)TotalItems / ItemsPerPage);

		public PaginationInfo(int totalItems, int itemsPerPage, int actualPage)
		{
			TotalItems = totalItems;
			ItemsPerPage = itemsPerPage;
			ActualPage = actualPage;
		}
	}
}
