namespace Projects.Api.Models.Requests
{
	public class AddTagsRequest
	{
		public List<string> Tags { get; set; } = new();
	}
}
