using System.ComponentModel.DataAnnotations;

namespace Projects.Api.Models.Requests
{
	public class AddMemberRequest
	{
		[EmailAddress]
		public string Email { get; set; } = default!;
		public string Role { get; set; } = default!;
	}
}
