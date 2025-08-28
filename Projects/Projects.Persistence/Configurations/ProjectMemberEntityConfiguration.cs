using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Projects.Persistence.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projects.Persistence.Configurations
{
	public partial class ProjectMemberEntityConfiguration : IEntityTypeConfiguration<ProjectMemberEntity>
	{
		public void Configure(EntityTypeBuilder<ProjectMemberEntity> builder)
		{
			builder.HasKey(m => m.Id);
		}
	}
}
