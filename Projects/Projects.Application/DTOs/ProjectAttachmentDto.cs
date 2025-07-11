using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projects.Application.DTOs
{
	public class ProjectAttachmentDto
	{
		public string FileName { get; set; } = default!;
		public string Url { get; set; } = default!;
	}
}
