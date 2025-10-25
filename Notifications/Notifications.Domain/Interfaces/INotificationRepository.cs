using Notifications.Domain.Aggregates.Root;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Notifications.Domain.Interfaces
{
	public interface INotificationRepository
	{
		Task<Notification> GetByIdAsync(Guid id, CancellationToken cancellationToken);
		Task AddAsync(Notification notification, CancellationToken cancellationToken);
		Task UpdateAsync(Notification notification, CancellationToken cancellationToken);
		Task DeleteAsync(Guid NotificationId, CancellationToken cancellationToken);
	}
}
