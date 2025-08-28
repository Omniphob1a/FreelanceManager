using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tasks.Application.Common.Pagination
{
	public class PaginationInfo
	{
		public int TotalItems { get; set; }
		public int ItemsPerPage { get; set; }
		public int ActualPage { get; set; } 
		public int TotalPages => (int)Math.Ceiling((double)TotalItems / ItemsPerPage);

		public PaginationInfo() { }

		public PaginationInfo(int totalItems, int itemsPerPage, int actualPage)
		{
			ActualPage = actualPage < 1 ? 1 : actualPage;
			ItemsPerPage = itemsPerPage is < 1 ? 10 : (itemsPerPage > 100 ? 100 : itemsPerPage);
			TotalItems = totalItems < 0 ? 0 : totalItems;
		}
	}
}
