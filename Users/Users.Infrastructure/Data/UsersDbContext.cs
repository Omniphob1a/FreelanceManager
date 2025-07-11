using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using Users.Domain.ValueObjects;
using Users.Infrastructure.Models;

namespace Users.Infrastructure.Data
{
	public class UsersDbContext : DbContext
	{
		public UsersDbContext(DbContextOptions<UsersDbContext> options) : base(options) { }

		public DbSet<UserData> Users { get; set; } = null!;
		public DbSet<RoleEntity> Roles { get; set; } = null!;
		public DbSet<PermissionEntity> Permissions { get; set; } = null!;
		public DbSet<UserRoleEntity> UserRoles { get; set; } = null!;
		public DbSet<RolePermissionEntity> RolePermissions { get; set; } = null!;

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.ApplyConfigurationsFromAssembly(typeof(UsersDbContext).Assembly);

			modelBuilder.Entity<PermissionEntity>().HasData(
				new PermissionEntity
				{
					Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
					Name = Permission.ManageUsers.Name
				},
				new PermissionEntity
				{
					Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
					Name = Permission.DeletePosts.Name
				},
				new PermissionEntity
				{
					Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
					Name = Permission.CreateUser.Name
				},
				new PermissionEntity
				{
					Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
					Name = Permission.DeleteUser.Name
				},
				new PermissionEntity
				{
					Id = Guid.Parse("55555555-5555-5555-5555-555555555555"),
					Name = Permission.UpdateUser.Name
				},
				new PermissionEntity
				{
					Id = Guid.Parse("66666666-6666-6666-6666-666666666666"),
					Name = Permission.ViewUser.Name
				}
			);

			modelBuilder.Entity<RoleEntity>().HasData(
				new RoleEntity
				{
					Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
					Name = "Admin"
				}
			);

			modelBuilder.Entity<RolePermissionEntity>().HasData(
				new { RoleId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), PermissionId = Guid.Parse("11111111-1111-1111-1111-111111111111") },
				new { RoleId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), PermissionId = Guid.Parse("22222222-2222-2222-2222-222222222222") },
				new { RoleId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), PermissionId = Guid.Parse("33333333-3333-3333-3333-333333333333") },
				new { RoleId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), PermissionId = Guid.Parse("44444444-4444-4444-4444-444444444444") },
				new { RoleId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), PermissionId = Guid.Parse("55555555-5555-5555-5555-555555555555") },
				new { RoleId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), PermissionId = Guid.Parse("66666666-6666-6666-6666-666666666666") }
			);



			var adminId = Guid.Parse("99999999-9999-9999-9999-999999999999");
			var adminPasswordHash = Convert.ToBase64String(SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes("AdminPass123")));

			modelBuilder.Entity<UserData>().HasData(new UserData
			{
				Id = adminId,
				Login = "administrator",
				PasswordHash = adminPasswordHash,
				Name = "Administrator",
				Email = "admin@example.com",
				Gender = 1,
				Birthday = DateTime.SpecifyKind(new DateTime(1990, 1, 1), DateTimeKind.Utc),
				CreatedAt = DateTime.SpecifyKind(new DateTime(2025, 5, 17, 0, 0, 0), DateTimeKind.Utc),
				CreatedBy = "System",
				Admin = true,
				RevokedOn = null
			});


			modelBuilder.Entity<UserRoleEntity>().HasData(new UserRoleEntity
			{
				UserId = adminId,
				RoleId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa")
			});

			base.OnModelCreating(modelBuilder);
		}
	}
}
