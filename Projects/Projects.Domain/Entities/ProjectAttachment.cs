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
		public Guid ProjectId { get; }


		public ProjectAttachment(string fileName, string url, Guid ProjectId)
		{
			Id = Guid.NewGuid();
			FileName = fileName ?? throw new ArgumentNullException(nameof(fileName));
			Url = url ?? throw new ArgumentNullException(nameof(url));
		}
		
		private ProjectAttachment(Guid id, string fileName, string url, Guid projectId)
		{
			Id = id;
			FileName = fileName ?? throw new ArgumentNullException(nameof(fileName));
			Url = url ?? throw new ArgumentNullException(nameof(url));
			ProjectId = projectId;
		}

		public static ProjectAttachment Load(Guid id, string fileName, string url, Guid projectId)
		{
			return new ProjectAttachment(id, fileName, url, projectId);
		}
	}
}
