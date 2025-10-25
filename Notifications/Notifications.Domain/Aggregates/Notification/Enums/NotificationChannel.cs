using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Notifications.Domain.Aggregates.Notification.Enums
{
	public enum NotificationChannel
	{
		InApp = 0,
		Email = 1,
		Sms = 2,
		Telegram = 3,
		Push = 4
	}
}
