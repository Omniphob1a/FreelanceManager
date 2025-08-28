using System.ComponentModel.DataAnnotations;

namespace Projects.Api.Models.Requests
{
	public class RemoveMemberRequest
	{
		[EmailAddress]
		public string Email { get; set; } = default!;
	}
}
