using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Notifications.Domain.Aggregates.Notification.Enums
{
	public enum DeliveryStatus
	{
		Pending = 0,
		Sending = 1,
		Sent = 2,
		Failed = 3,
		Dead = 4,
		Canceled = 5
	}
}
