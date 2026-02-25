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
	public class NotificationDeliveryEntityConfiguration : IEntityTypeConfiguration<NotificationDeliveryEntity>
	{
		public void Configure(EntityTypeBuilder<NotificationDeliveryEntity> builder)
		{
			builder.HasKey(d => d.Id);

			builder.Property(d => d.Channel)
				.HasConversion<int>()
				.IsRequired();
			builder.Property(d => d.Status)
				.HasConversion<int>()
				.IsRequired();

			builder.Property(d => d.Attempts)
				   .IsRequired();
			builder.Property(d => d.CreatedAt)
				   .IsRequired();
			builder.Property(d => d.UpdatedAt);
		}
	}
}
