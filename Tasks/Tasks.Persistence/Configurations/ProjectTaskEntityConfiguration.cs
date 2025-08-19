using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tasks.Persistence.Models;

public class ProjectTaskEntityConfiguration : IEntityTypeConfiguration<ProjectTaskEntity>
{
	public void Configure(EntityTypeBuilder<ProjectTaskEntity> builder)
	{
		builder.HasKey(x => x.Id);

		builder.Property(x => x.Id)
			.IsRequired();

		builder.Property(x => x.ProjectId)
			.IsRequired();

		builder.Property(x => x.Title)
			.IsRequired()
			.HasMaxLength(200);

		builder.Property(x => x.Description)
			.HasMaxLength(2000);

		builder.Property(x => x.AssigneeId)
			.IsRequired(false);

		builder.Property(x => x.ReporterId)
			.IsRequired();

		builder.Property(x => x.Status)
			.IsRequired();

		builder.Property(x => x.Priority)
			.IsRequired();

		builder.Property(x => x.TimeEstimatedTicks)
			.IsRequired();

		builder.Property(x => x.TimeSpentTicks)
			.IsRequired();

		builder.Property(x => x.DueDate)
			.IsRequired(false);

		builder.Property(x => x.IsBillable)
			.IsRequired();

		builder.Property(x => x.CreatedAt)
			.IsRequired();

		builder.Property(x => x.UpdatedAt)
			.IsRequired();

		builder.Property(x => x.CreatedBy)
			.IsRequired();

		builder.HasMany(x => x.TimeEntries)
			.WithOne()
			.HasForeignKey(te => te.TaskId)
			.OnDelete(DeleteBehavior.Cascade);

		builder.HasMany(x => x.Comments)
			.WithOne()
			.HasForeignKey(c => c.TaskId)
			.OnDelete(DeleteBehavior.Cascade);
	}
}
