using Ardalis.Specification;
using Projects.Domain.Entities;
using Projects.Persistence.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projects.Persistence.Specifications
{
	public class ProjectByIdWithMilestonesSpec : Specification<ProjectEntity>
	{
		public ProjectByIdWithMilestonesSpec(Guid projectId)
		{
			Query
				.Where(p => p.Id == projectId)
				.Include(p => p.Milestones)
				.EnableCache(nameof(ProjectByIdWithMilestonesSpec), projectId);
		}
	}
}
