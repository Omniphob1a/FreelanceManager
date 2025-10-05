namespace Tasks.Api.Models.Requests
{
	public class CreateProjectTaskRequest
	{
		public Guid ProjectId { get; set; }
		public string Title { get; set; } = string.Empty;
		public string? Description { get; set; }

		public TimeSpan? TimeEstimated { get; set; }   
		public DateTime? DueDate { get; set; }

		public bool IsBillable { get; set; }
		public decimal? HourlyRate { get; set; }      

		public int Priority { get; set; }             
		public Guid? AssigneeId { get; set; }          
	}
}
