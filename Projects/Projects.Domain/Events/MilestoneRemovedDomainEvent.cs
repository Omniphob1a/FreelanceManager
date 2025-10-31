using Projects.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projects.Domain.Events
{
	public record MilestoneRemovedDomainEvent(Guid ProjectId, ProjectMilestone Milestone) : DomainEvent(ProjectId, nameof(Project))
	{
		public override string EventType => "projects.milestone.removed";
		public override string? KafkaTopic => "projects";
		public override string? KafkaKey => $"{ProjectId}-milestone-{Milestone.Id}";
		public override bool IsTombstone => true;
	}
}
