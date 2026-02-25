using Notifications.Application.DTOs;
using Notifications.Application.Notifications.Commands.CreateNotification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Notifications.Application.Interfaces
{
	public interface IEventHandler
	{
		string EventType { get; }	
		Task<CreateNotificationCommand> HandleAsync(IncomingEventDto incoming, CancellationToken ct);
	}
}
