using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projects.Domain.Entities
{
	public class ProjectAttachment
	{
		public Guid Id { get; }
		public string FileName { get; }
		public string Url { get; }

		public ProjectAttachment(string fileName, string url)
		{
			Id = Guid.NewGuid();
			FileName = fileName ?? throw new ArgumentNullException(nameof(fileName));
			Url = url ?? throw new ArgumentNullException(nameof(url));
		}
	}
}
