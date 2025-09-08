using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tasks.Persistence.Models.ReadModels
{
	public class ProjectReadModel
	{
		public Guid Id { get; set; }
		public string Title { get; set; } = default!;
		public string Description { get; set; } = default!;
		public Guid OwnerId { get; set; }
		public string Category { get; set; } = default!;
		public DateTime CreatedAt { get; set; }
		public DateTime? ExpiresAt { get; set; }
		public int Status { get; set; }
		public decimal? BudgetMin { get; set; }
		public decimal? BudgetMax { get; set; }
		public string CurrencyCode { get; set; } = default!;
		public List<string> Tags { get; set; } = new();
		public bool IsExpired { get; set; }
	}
}
