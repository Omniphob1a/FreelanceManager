using Projects.Domain.Entities;
using Projects.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projects.Domain.Events
{
	public sealed record TagsAddedDomainEvent(Guid ProjectId, Tag Tag) : DomainEvent(ProjectId, nameof(Project))
	{
		public override string EventType => "projects.tags.added";
		public override string? KafkaTopic => "projects";
		public override string? KafkaKey => ProjectId.ToString();
	}
}
