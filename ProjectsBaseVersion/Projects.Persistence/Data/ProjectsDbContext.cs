using Microsoft.EntityFrameworkCore;
using Projects.Persistence.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projects.Persistence.Data
{
	public class ProjectsDbContext : DbContext
	{
		public ProjectsDbContext(DbContextOptions options) : base(options) {  }

		public DbSet<ProjectEntity> Projects { get; set; } = null!;
		public DbSet<ProjectMilestoneEntity> ProjectMilestones { get; set; } = null!;
		public DbSet<ProjectAttachmentEntity> ProjectAttachments { get; set; } = null!;

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);
			modelBuilder.ApplyConfigurationsFromAssembly(typeof(ProjectsDbContext).Assembly);
		}
	}
}
