using Ardalis.Specification;
using Projects.Persistence.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projects.Persistence.Specifications
{
	public class ProjectsOutOfDateSpec : Specification<ProjectEntity>
	{
		public ProjectsOutOfDateSpec(DateTime thresholdDate)
		{
			Query
				.Where(p => p.ExpiresAt.HasValue && p.ExpiresAt < thresholdDate)
				.Include(p => p.Milestones)
				.Include(p => p.Attachments);
		}
	}
}
