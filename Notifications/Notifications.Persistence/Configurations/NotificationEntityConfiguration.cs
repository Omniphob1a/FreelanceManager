using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Notifications.Persistence.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Notifications.Persistence.Configurations
{
	public class NotificationEntityConfiguration : IEntityTypeConfiguration<NotificationEntity>
	{
		public void Configure(EntityTypeBuilder<NotificationEntity> builder)
		{
			builder.HasKey(n => n.Id);
		}
	}
}
