using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projects.Application.DTOs
{
	public class PublicUserDto
	{
		public Guid Id { get; set; }
		public string Name { get; set; } = "";
		public string Login { get; set; } = "";
		public int Gender { get; set; }
		public DateTime? Birthday { get; set; }
	}
}
