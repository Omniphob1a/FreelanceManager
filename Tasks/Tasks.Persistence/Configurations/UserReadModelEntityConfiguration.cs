using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tasks.Persistence.Models.ReadModels;

namespace Tasks.Persistence.Configurations
{
	public class UserReadModelEntityConfiguration : IEntityTypeConfiguration<UserReadModel>
	{
		public void Configure(EntityTypeBuilder<UserReadModel> builder)
		{
			builder.ToTable("Users")
				.HasKey(u => u.Id);
		}
	}
}
