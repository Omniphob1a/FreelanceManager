using Ardalis.Specification;
using Projects.Persistence.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projects.Persistence.Specifications
{
	public class ProjectFullByIdSpec : Specification<ProjectEntity>
	{
		public ProjectFullByIdSpec(Guid ProjectId)
		{

			Query
				.Where(p => p.Id == ProjectId)
				.Include(p => p.Milestones)
				.Include(p => p.Attachments);
		}
	}
}
