using Projects.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projects.Domain.Events
{
	public record ProjectCompletedDomainEvent(Guid ProjectId) : DomainEvent(ProjectId, nameof(Project))
	{
		public override string EventType => "projects.completed";
		public override string? KafkaTopic => "projects";
		public override string? KafkaKey => ProjectId.ToString();
	}
}
