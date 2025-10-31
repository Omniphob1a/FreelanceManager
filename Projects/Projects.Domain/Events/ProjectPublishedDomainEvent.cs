using Projects.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projects.Domain.Events
{
	public record ProjectPublishedDomainEvent(Guid ProjectId, DateTime ExpiresAt) : DomainEvent(ProjectId, nameof(Project))
	{
		public override string EventType => "projects.published";
		public override string? KafkaTopic => "projects";
		public override string? KafkaKey => ProjectId.ToString();
	}
}
