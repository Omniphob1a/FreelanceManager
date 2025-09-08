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
	public class ProjectReadModelEntityConfiguration : IEntityTypeConfiguration<ProjectReadModel>
	{
		public void Configure(EntityTypeBuilder<ProjectReadModel> builder)
		{
			builder.ToTable("Projects")
				.HasKey(p => p.Id);
		}
	}
}
