using FluentResults;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Notifications.Application.Notifications.Commands.SendNotification
{
	public record SendNotificationCommand(Guid NotificationId) : IRequest<Result<Unit>>;
}
