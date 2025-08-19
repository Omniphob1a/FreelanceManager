namespace Tasks.Api.Models.Requests
{
	public class UpdateProjectTaskRequest
	{
		public string Title { get; set; }
		public string? Description { get; set; }

		public decimal EstimateValue { get; set; }
		public int EstimateUnit { get; set; }

		public DateTime? DueDate { get; set; }
		public bool IsBillable { get; set; }

		public decimal Amount { get; set; }
		public string? Currency { get; set; }

		public int Priority { get; set; } 
	}
}
