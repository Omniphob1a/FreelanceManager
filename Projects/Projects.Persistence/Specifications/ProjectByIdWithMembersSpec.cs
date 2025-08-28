using Ardalis.Specification;
using Projects.Persistence.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projects.Persistence.Specifications
{
	public class ProjectByIdWithMembersSpec : Specification<ProjectEntity>
	{
		public ProjectByIdWithMembersSpec(Guid projectId)
		{
			Query
				.Where(p => p.Id == projectId)
				.Include(p => p.Members)
				.EnableCache(nameof(ProjectByIdWithMembersSpec), projectId);
		}
	}
}
