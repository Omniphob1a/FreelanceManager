using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projects.Persistence.Models
{
	public class ProjectAttachmentEntity
	{
		public Guid Id { get; }
		public Guid ProjectId { get; }
		public string FileName { get; } = default!;
		public string Url { get; } = default!;
	}
}
