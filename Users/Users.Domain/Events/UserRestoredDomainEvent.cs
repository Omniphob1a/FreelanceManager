using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tasks.Domain.Aggregate.Events;
using Users.Domain.Entities;
using Users.Domain.Interfaces;

namespace Users.Domain.Events
{
	public record UserRestoredDomainEvent(Guid UserId) : DomainEvent(UserId, nameof(User))
	{
		public override string EventType => "users.restored";
		public override string? KafkaTopic => "users";
		public override string? KafkaKey => UserId.ToString();
	}
}
