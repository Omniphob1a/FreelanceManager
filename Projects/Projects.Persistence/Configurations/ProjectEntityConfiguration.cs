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

			builder.Property(p => p.CreatedAt).IsRequired();

			builder.Property(p => p.ExpiresAt);

			builder.Property(p => p.Category)
				.HasMaxLength(100)
				.IsRequired();

			builder.Property(p => p.BudgetMin)
				.HasColumnName("BudgetMin")
				.HasPrecision(18, 2)
				.IsRequired();

			builder.Property(p => p.BudgetMax)
				.HasColumnName("BudgetMax")
				.HasPrecision(18, 2)
				.IsRequired();

			builder.Property(p => p.CurrencyCode)
				.HasColumnName("CurrencyCode")
				.HasMaxLength(10)
				.IsRequired();

			builder.Property(p => p.Tags)
				.HasColumnName("Tags")
				.HasMaxLength(1000) 
				.IsRequired();
		
			builder.HasMany(p => p.Milestones)
				.WithOne()
				.HasForeignKey(m => m.ProjectId)
				.OnDelete(DeleteBehavior.Cascade);

			builder.HasMany(p => p.Attachments)
				.WithOne()
				.HasForeignKey(a => a.ProjectId)
				.OnDelete(DeleteBehavior.Cascade);

			builder.Property(p => p.Title)
				.HasMaxLength(200)
				.IsRequired();

			builder.Property(p => p.Description)
				.HasMaxLength(2000)
				.IsRequired();

			builder.Property(p => p.Status)
				.IsRequired();
		}
	}
}
