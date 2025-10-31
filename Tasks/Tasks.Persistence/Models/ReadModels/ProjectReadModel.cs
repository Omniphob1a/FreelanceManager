using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tasks.Persistence.Models.ReadModels
{
	public class ProjectReadModel
	{
		public Guid Id { get; set; }
		public string Title { get; set; } = default!;
		public Guid OwnerId { get; set; }
	}
}
