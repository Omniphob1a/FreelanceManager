using Notifications.Domain.Aggregates.Notification.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Notifications.Persistence.Models.Entities
{
	public partial class NotificationEntity
	{
		public Guid Id { get; set; } 

		public Guid? EventId { get; set; }

		public Guid UserId { get; set; }

		public string TemplateKey { get; set; } 

		public string? PayloadRaw { get; set; }
		public string? PayloadRendered { get; set; }

		public DateTimeOffset CreatedAt { get; set; } 
		public DateTimeOffset? ReadAt { get; set; }
		public List<NotificationDeliveryEntity> Deliveries { get; set; } = new();
		public bool IsTombstone { get; set; } = false;
	}
}
