namespace Tasks.Api.Models.Requests
{
	public class AddCommentRequest
	{
		public Guid AuthorId { get; set; }
		public string Text { get; set; } = default!;
	}
}
