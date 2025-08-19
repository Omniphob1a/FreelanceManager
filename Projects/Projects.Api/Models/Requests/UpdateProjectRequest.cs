namespace Projects.Api.Models.Requests
{
	public class UpdateProjectRequest
	{
		public string Title { get; set; } = default!;
		public string Description { get; set; } = default!;
		public Guid OwnerId { get; set; }
		public decimal BudgetMin { get; set; }
		public decimal BudgetMax { get; set; }
		public string CurrencyCode { get; set; } = default!;
		public string Category { get; set; } = default!;
		public List<string> Tags { get; set; } = new();
	}
}
