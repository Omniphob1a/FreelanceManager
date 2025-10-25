using Microsoft.EntityFrameworkCore;
using Notifications.Persistence.Models.Entities;
using Notifications.Persistence.Models.ReadModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Notifications.Persistence.Data
{
	public class NotificationsDbContext : DbContext
	{
		public NotificationsDbContext(DbContextOptions<NotificationsDbContext> options) : base(options)	{}

		public DbSet<NotificationEntity> Notifications { get; set; } = null!;
		public DbSet<UserReadModel> Users { get; set; } = null!;


		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.ApplyConfigurationsFromAssembly(typeof(NotificationsDbContext).Assembly);
			base.OnModelCreating(modelBuilder);
		}
	}
}
