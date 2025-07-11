using System.ComponentModel.DataAnnotations;

namespace Projects.Api.Models.Requests
{
	public class AddAttachmentRequest
	{
		public IFormFile File { get; set; }
	}
}
