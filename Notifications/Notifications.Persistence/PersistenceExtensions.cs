using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Notifications.Domain.Interfaces;
using Notifications.Persistence.Data;
using Notifications.Persistence.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Notifications.Persistence
{
	public static class PersistenceExtensions
	{
		public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
		{
			var connectionString = configuration.GetConnectionString(nameof(NotificationsDbContext))
				?? throw new ArgumentNullException("Connection string 'NotificationsDbContext' not found.");

			services.AddDbContext<NotificationsDbContext>(options => options.UseNpgsql(connectionString));
			services.AddScoped<INotificationRepository, NotificationRepository>();
				
			return services;
		}
	}
}
