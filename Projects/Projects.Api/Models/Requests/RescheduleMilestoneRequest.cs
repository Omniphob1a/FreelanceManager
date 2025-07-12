namespace Projects.Api.Models.Requests
{
	public class RescheduleMilestoneRequest
	{
		public Guid MilestoneId { get; set; }
		public DateTime NewDueDate { get; set; }
	}
}
