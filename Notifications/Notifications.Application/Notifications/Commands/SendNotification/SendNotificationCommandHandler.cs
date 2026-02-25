using FluentResults;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Notifications.Application.Notifications.Commands.SendNotification
{
	public class SendNotificationCommandHandler : IRequestHandler<SendNotificationCommand, Result<Unit>>
	{
		public SendNotificationCommandHandler()
		{
		}

		public async Task<Result<Unit>> Handle(SendNotificationCommand request, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}
	}
}
