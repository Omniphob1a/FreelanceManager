using Ardalis.Specification;
using Projects.Domain.Entities;
using Projects.Persistence.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projects.Persistence.Specifications
{
	public class ProjectByIdWithAttachmentsSpec : Specification<ProjectEntity>
	{
		public ProjectByIdWithAttachmentsSpec(Guid projectId)
		{
			Query
				.Where(p => p.Id == projectId)
				.Include(p => p.Attachments)
				.EnableCache(nameof(ProjectByIdWithAttachmentsSpec), projectId);
		}
	}
}
