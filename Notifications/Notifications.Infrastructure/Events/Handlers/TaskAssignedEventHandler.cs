using Notifications.Application.DTOs;
using Notifications.Application.Interfaces;
using Notifications.Application.Notifications.Commands.CreateNotification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Notifications.Infrastructure.Events.Handlers
{
	public class TaskAssignedEventHandler : IEventHandler
	{
		public string EventType => "tasks.assigned";

		public async Task<CreateNotificationCommand> HandleAsync(IncomingEventDto incoming, CancellationToken ct)
		{

			var payload = JsonDocument.Parse(incoming.Payload!);
			var payloadRoot = payload.RootElement;

			if (!payloadRoot.TryGetProperty("assigneeId", out var aProp) ||
				!Guid.TryParse(aProp.GetString(), out var assigneeId))
				throw new InvalidOperationException("Invalid assigneeId");

			if (!payloadRoot.TryGetProperty("taskId", out var tProp) ||
				!Guid.TryParse(tProp.GetString(), out var taskId))
				throw new InvalidOperationException("Invalid taskId");


			//channel в будущем можно реализовать с UserPrefs, получая из локальной проекции юзера
			return new CreateNotificationCommand(
				EventId: incoming.Id,
				UserId: assigneeId,
				Channel: 1,
				TemplateKey: "task.assigned",
				Payload: JsonSerializer.Serialize(new { TaskId = taskId })
			);
		}
	}
}
