using Notifications.Domain.Aggregates.Notification.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Notifications.Domain.Aggregates.Notification.Entities
{
	public sealed class NotificationDelivery
	{
		public Guid Id { get; private set; }
		public Guid NotificationId { get; private set; }
		public NotificationChannel Channel { get; private set; }
		public DeliveryStatus Status { get; private set; } = DeliveryStatus.Pending;
		public int Attempts { get; private set; } = 0;
		public DateTimeOffset? ScheduledAt { get; private set; }
		public DateTimeOffset? SentAt { get; private set; }
		public string? ProviderMessageId { get; private set; }
		public string? LastError { get; private set; }


		private NotificationDelivery(NotificationChannel channel, DateTimeOffset? scheduledAt = null)
		{
			Id = Guid.NewGuid();
			Channel = channel;
			Status = DeliveryStatus.Pending;
			ScheduledAt = scheduledAt;
		}
		public static NotificationDelivery Create(NotificationChannel channel, DateTimeOffset? scheduledAt = null)
		{
			if (!Enum.IsDefined(typeof(NotificationChannel), channel))
				throw new ArgumentException("Invalid notification channel.", nameof(channel));

			return new NotificationDelivery(channel, scheduledAt);
		}

		public void MarkSending()
		{
			if (Status == DeliveryStatus.Sent || Status == DeliveryStatus.Dead)
				throw new InvalidOperationException("Cannot mark sending.");
			Status = DeliveryStatus.Sending;
			ScheduledAt = null;
		}

		public void MarkSent(DateTimeOffset sentAt, string? providerMessageId = null)
		{
			Status = DeliveryStatus.Sent;
			SentAt = sentAt;
			ProviderMessageId = providerMessageId;
		}
		public void MarkCanceled(string reason)
		{
			Status = DeliveryStatus.Canceled;
			LastError = reason;
		}

		public void MarkFailed(string errorMessage, Func<int, TimeSpan?> backoffStrategy, int maxAttempts)
		{
			if (Status == DeliveryStatus.Sent || Status == DeliveryStatus.Dead) return;
			Attempts++;
			LastError = errorMessage;
			var backoff = backoffStrategy(Attempts);
			if (backoff.HasValue && Attempts < maxAttempts)
			{
				ScheduledAt = DateTimeOffset.UtcNow.Add(backoff.Value);
				Status = DeliveryStatus.Pending;
			}
			else
			{
				Status = DeliveryStatus.Dead;
			}
		}

		public bool CanRetry(int maxAttempts) => Attempts < maxAttempts && Status != DeliveryStatus.Dead && Status != DeliveryStatus.Sent;
	}
}
