using System.ComponentModel.DataAnnotations;

namespace Projects.Api.Models.Requests
{
	public class AddMemberRequest
	{
		public string Login { get; set; } = default!;
		public string Role { get; set; } = default!;
	}
}
