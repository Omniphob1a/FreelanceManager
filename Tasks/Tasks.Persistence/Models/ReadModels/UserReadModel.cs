using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tasks.Persistence.Models.ReadModels
{
	public class UserReadModel
	{
		public Guid Id { get; set; }
		public string Name { get; set; } = "";
		public string Login { get; set; } = "";
		public int Gender { get; set; }
		public DateTime Birthday { get; set; }
	}
}
