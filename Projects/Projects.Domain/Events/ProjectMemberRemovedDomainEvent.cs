using Projects.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projects.Domain.Events
{
	public record ProjectMemberRemovedDomainEvent(Guid ProjectId, Guid MemberId, Guid UserId) : DomainEvent(ProjectId, nameof(Project))
	{
		public override string EventType => "projects.member.removed";
		public override string? KafkaTopic => "projects";
		public override string? KafkaKey => $"{ProjectId}-member-{MemberId}";
		public override bool IsTombstone => true;
	}
}
