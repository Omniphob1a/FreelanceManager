using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Projects.Persistence.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projects.Infrastructure.Configurations
{
	public partial class ProjectMilestoneEntityConfiguration : IEntityTypeConfiguration<ProjectMilestoneEntity>
	{
		public void Configure(EntityTypeBuilder<ProjectMilestoneEntity> builder)
		{
			builder.HasKey(pm => pm.Id);
		}
	}
}
