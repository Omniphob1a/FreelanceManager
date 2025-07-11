using Projects.Domain.Entities.ProjectService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projects.Domain.Events
{
	public record MilestoneAddedDomainEvent(Guid ProjectId, ProjectMilestone Milestone)	: DomainEvent(ProjectId);
}
