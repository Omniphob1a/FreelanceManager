using Projects.Domain.Entities;
using System;

namespace Projects.Domain.Events
{
	public record ProjectConfirmRequestedDomainEvent(Guid ProjectId, Guid UserId)
		: DomainEvent(ProjectId, nameof(Project))
	{
		public override string EventType => "projects.confirm-requested";

		public override string? KafkaTopic => "user-confirm-requests";

		public override string? KafkaKey => ProjectId.ToString();
	}
}
