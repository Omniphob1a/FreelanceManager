using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tasks.Persistence.Models;

namespace Tasks.Persistence.Configurations
{
	public class TimeEntryEntityConfiguration : IEntityTypeConfiguration<TimeEntryEntity>
	{
		public void Configure(EntityTypeBuilder<TimeEntryEntity> builder)
		{
			builder.HasKey(x => x.Id);
		}
	}
}
