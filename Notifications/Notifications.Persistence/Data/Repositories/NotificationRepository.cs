using Notifications.Domain.Aggregates.Root;
using Notifications.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Notifications.Persistence.Data.Repositories
{
	public class NotificationRepository : INotificationRepository
	{
		public async Task AddAsync(Notification notification, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}

		public async Task DeleteAsync(Guid NotificationId, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}

		public async Task<Notification> GetByIdAsync(Guid id, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}

		public async Task UpdateAsync(Notification notification, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}
	}
}
