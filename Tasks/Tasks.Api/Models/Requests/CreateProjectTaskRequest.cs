namespace Tasks.Api.Models.Requests
{
	public class CreateProjectTaskRequest
	{
		public Guid ProjectId { get; set; }
		public string Title { get; set; } = string.Empty;
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
