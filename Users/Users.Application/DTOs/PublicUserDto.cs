using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Users.Application.DTOs
{
	public class PublicUserDto
	{
		public Guid Id { get; init; }
		public string Name { get; init; } = "";
		public string Login { get; init; } = ""; 
		public int Gender { get; init; }
		public DateTime? Birthday { get; init; }
	}
}
