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
	public partial class ProjectEntityConfiguration : IEntityTypeConfiguration<ProjectEntity>
	{
		public void Configure(EntityTypeBuilder<ProjectEntity> builder)
		{
			builder.HasKey(p => p.Id);

			builder.HasMany(p => p.Milestones)
				.WithOne()
				.HasForeignKey(m => m.ProjectId)
				.OnDelete(DeleteBehavior.Cascade);

			builder.HasMany(p => p.Attachments)
				.WithOne()
				.HasForeignKey(a => a.ProjectId)
				.OnDelete(DeleteBehavior.Cascade);

			builder.Property(p => p.Tags)
				.HasConversion(
					v => string.Join(",", v),
					v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()
				);

			builder.Property(p => p.BudgetMin).HasColumnName("BudgetMin");
			builder.Property(p => p.BudgetMax).HasColumnName("BudgetMax");
			builder.Property(p => p.Currency).HasColumnName("Currency").IsRequired();
		}
	}
}
