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
	public class ProjectByIdSpec : Specification<ProjectEntity>
	{
		public ProjectByIdSpec(Guid projectId)
		{
			Query.Where(p => p.Id == projectId);
		}
	}
}
