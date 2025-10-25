using FluentResults;
using MediatR;
using Notifications.Domain.Aggregates.Notification.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Notifications.Application.Notifications.Commands.CreateNotification
{
	public record CreateNotificationCommand(
		Guid EventId,
		Guid UserId,
		int Channel,
		string TemplateKey,
		string? Payload = null
	) : IRequest<Result<Guid>>;
}
