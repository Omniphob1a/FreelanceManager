using System.ComponentModel.DataAnnotations;

namespace Projects.Api.Models.Requests
{
	public class RemoveMemberRequest
	{
		public string Login { get; set; } = default!;
	}
}
