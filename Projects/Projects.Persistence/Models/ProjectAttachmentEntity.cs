using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projects.Persistence.Models
{
	public class ProjectAttachmentEntity
	{
		public Guid Id { get; set; }
		public Guid ProjectId { get; set; }
		public string FileName { get; set; } = default!;
		public string Url { get; set; } = default!;
	}
}
