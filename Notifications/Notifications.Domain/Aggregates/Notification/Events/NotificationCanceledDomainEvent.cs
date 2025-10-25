using Notifications.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Notifications.Domain.Aggregates.Notification.Events
{

	public record NotificationCanceledDomainEvent(
		Guid AggregateId,
		Guid NotificationId,
		string Reason
	) : DomainEvent(AggregateId);
}
