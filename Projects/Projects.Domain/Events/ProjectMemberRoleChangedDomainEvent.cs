using Projects.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projects.Domain.Events
{
	public record ProjectMemberRoleChangedDomainEvent(Guid ProjectId, Guid MemberId, Guid UserId, string NewRole) : DomainEvent(ProjectId, nameof(Project))
	{
		public override string EventType => "projects.member.role_changed";
		public override string? KafkaTopic => "projects";
		public override string? KafkaKey => $"{ProjectId}-member-{MemberId}";
	}
}

