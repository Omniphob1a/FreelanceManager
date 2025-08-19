using Ardalis.Specification;
using Projects.Persistence.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projects.Persistence.Specifications
{
	public class AllProjectsSpec : Specification<ProjectEntity>
	{
		public AllProjectsSpec() 
		{ 
			Query
				.Include(p => p.Milestones)
				.Include(p => p.Attachments);
		}	
	}
}
