using Notifications.Domain.Aggregates.Notification.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Notifications.Persistence.Models.Entities
{
	public class NotificationEntity
	{
		public Guid Id { get; }
		public Guid EventId { get; set; }
		public Guid UserId { get; set; }
		public int Channel { get; set; } = 0;
		public string TemplateKey { get; set; } = null!;
		public string? Payload { get; set; }
		public int Status { get; set; } = 0;
		public int Attempts { get; set; } = 0;
		public DateTimeOffset? ScheduledAt { get; set; }
		public DateTimeOffset CreatedAt { get; set; } 
		public DateTimeOffset? SentAt { get; set; }
		public DateTimeOffset? ReadAt { get; set; }
		public string? LastError { get; set; }
	}
}
