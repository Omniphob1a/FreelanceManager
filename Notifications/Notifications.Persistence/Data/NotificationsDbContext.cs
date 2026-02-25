using Microsoft.EntityFrameworkCore;
using Notifications.Domain.Aggregates.Notification.Entities;
using Notifications.Persistence.Models.Entities;
using Notifications.Persistence.Models.ReadModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Notifications.Persistence.Models.Entities.NotificationEntity;

namespace Notifications.Persistence.Data
{
	public class NotificationsDbContext : DbContext
	{
		public NotificationsDbContext(DbContextOptions<NotificationsDbContext> options) : base(options)	{}

		public DbSet<NotificationEntity> Notifications { get; set; } = null!;
		public DbSet<NotificationDeliveryEntity> NotificationDeliveries { get; set; } = null!;
		public DbSet<UserReadModel> Users { get; set; } = null!;
		public DbSet<MemberReadModel> ProjectMembers { get; set; } = null!;
		public DbSet<IncomingEvent> IncomingEvents { get; set; } = null!;
		public DbSet<OutboxMessage> OutboxMessages { get; set; } = null!;


		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.ApplyConfigurationsFromAssembly(typeof(NotificationsDbContext).Assembly);
			base.OnModelCreating(modelBuilder);
		}
	}
}
