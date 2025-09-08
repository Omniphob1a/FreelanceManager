using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Tasks.Persistence.Models.ReadModels;

namespace Tasks.Persistence.Configurations
{
	public class MemberReadModelEntityConfiguration : IEntityTypeConfiguration<MemberReadModel>
	{
		public void Configure(EntityTypeBuilder<MemberReadModel> builder)
		{

			builder.ToTable("Members");
			builder.HasKey(x => new { x.ProjectId, x.UserId });  
			builder.Property(x => x.Role).IsRequired();
			builder.HasIndex(x => x.ProjectId);                  
		}
	}
}
