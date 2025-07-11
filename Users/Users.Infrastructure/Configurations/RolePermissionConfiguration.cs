using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Users.Infrastructure;
using Users.Domain.Entities;
using Users.Infrastructure.Models;

public partial class RolePermissionConfiguration
    : IEntityTypeConfiguration<RolePermissionEntity>
{
	public void Configure(EntityTypeBuilder<RolePermissionEntity> builder)
	{
		builder.HasKey(x => new { x.RoleId, x.PermissionId });

		builder
			.HasOne(x => x.Role)
			.WithMany(r => r.RolePermissions)
			.HasForeignKey(x => x.RoleId);

		builder
			.HasOne(x => x.Permission)
			.WithMany(p => p.RolePermissions)
			.HasForeignKey(x => x.PermissionId);
	}

}
