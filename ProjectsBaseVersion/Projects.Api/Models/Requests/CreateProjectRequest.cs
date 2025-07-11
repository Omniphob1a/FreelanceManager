namespace Projects.Api.Models.Requests
{
	public class CreateProjectRequest
	{
		public string Title { get; set; } = default!;
		public string Description { get; set; } = default!;
		public decimal BudgetMin { get; set; }
		public decimal BudgetMax { get; set; }
		public string Currency { get; set; } = default!;
		public string Category { get; set; } = default!;
		public List<string> Tags { get; set; } = new();
	}
}
