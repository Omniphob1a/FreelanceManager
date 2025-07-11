namespace Projects.Api.Models.Requests
{
	public class AddMilestoneRequest
	{
		public string Title { get; set; } = default!;
		public DateTime DueDate { get; set; }
	}
}
