namespace Tasks.Api.Models.Requests
{
	public class AddTimeEntryRequest
	{
		public DateTime StartedAt { get; set; }
		public DateTime EndedAt { get; set; }

		public string? Description { get; set; }
		public bool IsBillable { get; set; }

		public decimal Amount { get; set; }
		public string? Currency { get; set; }
	}
}
