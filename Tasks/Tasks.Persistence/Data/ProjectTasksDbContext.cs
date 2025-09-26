using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tasks.Application.DTOs;
using Tasks.Persistence.Models;
using Tasks.Persistence.Models.ReadModels;

namespace Tasks.Persistence.Data
{
	public class ProjectTasksDbContext : DbContext
	{
		public ProjectTasksDbContext(DbContextOptions<ProjectTasksDbContext> options) : base(options) { }

		public DbSet<ProjectTaskEntity> Tasks { get; set; } = null!;
		public DbSet<CommentEntity> Comments { get; set; } = null!;
		public DbSet<TimeEntryEntity> TimeEnries { get; set; } = null!;
		public DbSet<UserReadModel> Users { get; set; } = null!;
		public DbSet<MemberReadModel> ProjectMembers { get; set; } = null!;

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.ApplyConfigurationsFromAssembly(typeof(ProjectTasksDbContext).Assembly);
			base.OnModelCreating(modelBuilder);
		}
	}
}
