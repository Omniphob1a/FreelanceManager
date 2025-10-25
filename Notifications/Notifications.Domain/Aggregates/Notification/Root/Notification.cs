using Notifications.Domain.Aggregates.Notification.Entities;
using Notifications.Domain.Aggregates.Notification.Enums;
using Notifications.Domain.Aggregates.Notification.Events;
using Notifications.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Notifications.Domain.Aggregates.Root
{
	public class Notification : AggregateRoot
	{
		public Guid Id { get; private set; }
		public Guid EventId { get; private set; }
		public Guid UserId { get; private set; }
		public string TemplateKey { get; private set; } = null!;
		public string? Payload { get; private set; }
		public DateTimeOffset CreatedAt { get; private set; }
		public DateTimeOffset? ReadAt { get; private set; }

		private readonly List<NotificationDelivery> _deliveries = new();
		public IReadOnlyCollection<NotificationDelivery> Deliveries => _deliveries.AsReadOnly();


		private Notification(Guid eventId, Guid userId, string templateKey, string? payload)
		{
			Id = Guid.NewGuid();
			EventId = eventId;
			UserId = userId;
			TemplateKey = templateKey;
			Payload = payload;
			CreatedAt = DateTimeOffset.UtcNow;
		}


		public static Notification Create(Guid eventId, Guid userId, string templateKey, string? payload = null)
		{
			if (eventId == Guid.Empty) throw new ArgumentException("EventId must be provided", nameof(eventId));
			if (userId == Guid.Empty) throw new ArgumentException("UserId must be provided", nameof(userId));
			if (string.IsNullOrWhiteSpace(templateKey)) throw new ArgumentException("TemplateKey must be provided", nameof(templateKey));

			var n = new Notification(eventId, userId, templateKey, payload);

			// Формируем DeliveryInfo из текущих (еще нет) deliveries — это удобно, если Create сразу создаёт deliveries
			var deliveries = n.Deliveries.Select(d => new DeliveryInfo(d.Id, d.Channel, d.ScheduledAt)).ToArray();

			n.AddDomainEvent(new NotificationCreatedDomainEvent(
				AggregateId: n.Id,
				NotificationId: n.Id,
				UserId: n.UserId,
				TemplateKey: n.TemplateKey,
				Payload: n.Payload,
				Deliveries: deliveries
			));

			return n;
		}


		public void AddDelivery(NotificationChannel channel, DateTimeOffset? scheduledAt = null)
		{
			if (_deliveries.Any(d => d.Channel == channel && d.Status != DeliveryStatus.Dead && d.Status != DeliveryStatus.Canceled))
				return; // либо throw в зависимости от требований

			var delivery = NotificationDelivery.Create(channel, scheduledAt);

			// Устанавливать FK вручную не требуется — EF сделает это при добавлении в контекст, но поле NotificationId присутствует в entity.
			_deliveries.Add(delivery);

			// Событие о запланированной доставке (помимо общего NotificationCreated)
			if (scheduledAt.HasValue)
			{
				AddDomainEvent(new DeliveryScheduledDomainEvent(
					AggregateId: Id,
					DeliveryId: delivery.Id,
					Channel: delivery.Channel,
					ScheduledAt: scheduledAt.Value
				));
			}
			else
			{
				// Можно все равно уведомить, что delivery создан (без schedule)
				AddDomainEvent(new DeliveryScheduledDomainEvent(
					AggregateId: Id,
					DeliveryId: delivery.Id,
					Channel: delivery.Channel,
					ScheduledAt: DateTimeOffset.UtcNow // указываем текущее как индикатор "ready"
				));
			}
		}

		public NotificationDelivery? GetDelivery(Guid deliveryId) =>
			_deliveries.FirstOrDefault(d => d.Id == deliveryId);

		public void MarkRead(DateTimeOffset readAt)
		{
			if (ReadAt.HasValue) return;
			ReadAt = readAt;

			AddDomainEvent(new NotificationReadDomainEvent(
				AggregateId: Id,
				NotificationId: Id,
				UserId: UserId,
				ReadAt: readAt
			));
		}

		public void CancelAllDeliveries(string reason)
		{
			foreach (var d in _deliveries.Where(x => x.Status == DeliveryStatus.Pending || x.Status == DeliveryStatus.Failed || x.Status == DeliveryStatus.Sending))
			{
				d.MarkCanceled(reason);
			}

			AddDomainEvent(new NotificationCanceledDomainEvent(
				AggregateId: Id,
				NotificationId: Id,
				Reason: reason
			));
		}

		// Возвращает готовые к отправке deliveries (для worker) — предпочтительно отбирать через репозиторий/SQL
		public IEnumerable<NotificationDelivery> GetDeliveriesReadyToSend(DateTimeOffset now)
		{
			return _deliveries.Where(d =>
				(d.Status == DeliveryStatus.Pending) &&
				(d.ScheduledAt == null || d.ScheduledAt <= now)
			);
		}

		public bool HasPendingDeliveries() => _deliveries.Any(d => d.Status == DeliveryStatus.Pending);

		internal void MarkDeliverySending(Guid deliveryId)
		{
			var d = GetDeliveryOrThrow(deliveryId);
			d.MarkSending();

			AddDomainEvent(new DeliverySendingDomainEvent(
				AggregateId: Id,
				DeliveryId: d.Id,
				Channel: d.Channel
			));
		}

		internal void MarkDeliverySent(Guid deliveryId, DateTimeOffset sentAt, string? providerMessageId = null)
		{
			var d = GetDeliveryOrThrow(deliveryId);
			d.MarkSent(sentAt, providerMessageId);

			AddDomainEvent(new DeliverySentDomainEvent(
				AggregateId: Id,
				DeliveryId: d.Id,
				Channel: d.Channel,
				SentAt: sentAt,
				ProviderMessageId: providerMessageId
			));
		}

		internal void MarkDeliveryFailed(Guid deliveryId, string error, Func<int, TimeSpan?> backoffStrategy, int maxAttempts)
		{
			var d = GetDeliveryOrThrow(deliveryId);

			// предыдущий attempts увеличится внутри MarkFailed
			d.MarkFailed(error, backoffStrategy, maxAttempts);

			if (d.Status == DeliveryStatus.Dead)
			{
				AddDomainEvent(new DeliveryDeadDomainEvent(
					AggregateId: Id,
					DeliveryId: d.Id,
					Channel: d.Channel,
					Reason: d.LastError ?? error
				));
			}
			else
			{
				AddDomainEvent(new DeliveryFailedDomainEvent(
					AggregateId: Id,
					DeliveryId: d.Id,
					Channel: d.Channel,
					Attempts: d.Attempts,
					Error: d.LastError ?? error
				));
			}
		}

		private NotificationDelivery GetDeliveryOrThrow(Guid deliveryId)
		{
			var d = GetDelivery(deliveryId);
			if (d == null) throw new InvalidOperationException($"Delivery {deliveryId} not found in notification {Id}");
			return d;
		}
	}
}
