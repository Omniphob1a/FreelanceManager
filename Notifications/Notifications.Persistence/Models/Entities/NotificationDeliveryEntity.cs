namespace Notifications.Persistence.Models.Entities
{
	public class NotificationDeliveryEntity
	{
		public Guid Id { get; set; }
		public Guid NotificationId { get; set; }
		public int Channel { get; set; }
		public int Status { get; set; }

		public int Attempts { get; set; }

		public DateTimeOffset? ScheduledAt { get; set; }
		public DateTimeOffset? NextAttemptAt { get; set; }
		public DateTimeOffset? SentAt { get; set; }

		public string? ProviderMessageId { get; set; }
		public string? LastError { get; set; }

		public DateTimeOffset CreatedAt { get; set; }
		public DateTimeOffset? UpdatedAt { get; set; }
	}
}
