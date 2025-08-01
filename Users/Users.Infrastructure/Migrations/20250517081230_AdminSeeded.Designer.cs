﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Users.Infrastructure.Data;

#nullable disable

namespace Users.Infrastructure.Migrations
{
    [DbContext(typeof(UsersDbContext))]
    [Migration("20250517081230_AdminSeeded")]
    partial class AdminSeeded
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.15")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Users.Infrastructure.Models.PermissionEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Permissions");

                    b.HasData(
                        new
                        {
                            Id = new Guid("11111111-1111-1111-1111-111111111111"),
                            Name = "ManageUsers"
                        },
                        new
                        {
                            Id = new Guid("22222222-2222-2222-2222-222222222222"),
                            Name = "DeletePosts"
                        },
                        new
                        {
                            Id = new Guid("33333333-3333-3333-3333-333333333333"),
                            Name = "CreateUser"
                        },
                        new
                        {
                            Id = new Guid("44444444-4444-4444-4444-444444444444"),
                            Name = "DeleteUser"
                        },
                        new
                        {
                            Id = new Guid("55555555-5555-5555-5555-555555555555"),
                            Name = "UpdateUser"
                        },
                        new
                        {
                            Id = new Guid("66666666-6666-6666-6666-666666666666"),
                            Name = "ViewUser"
                        });
                });

            modelBuilder.Entity("Users.Infrastructure.Models.RoleEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Roles");

                    b.HasData(
                        new
                        {
                            Id = new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                            Name = "Admin"
                        });
                });

            modelBuilder.Entity("Users.Infrastructure.Models.RolePermissionEntity", b =>
                {
                    b.Property<Guid>("RoleId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("PermissionId")
                        .HasColumnType("uuid");

                    b.HasKey("RoleId", "PermissionId");

                    b.HasIndex("PermissionId");

                    b.ToTable("RolePermissions");

                    b.HasData(
                        new
                        {
                            RoleId = new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                            PermissionId = new Guid("11111111-1111-1111-1111-111111111111")
                        },
                        new
                        {
                            RoleId = new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                            PermissionId = new Guid("22222222-2222-2222-2222-222222222222")
                        },
                        new
                        {
                            RoleId = new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                            PermissionId = new Guid("33333333-3333-3333-3333-333333333333")
                        },
                        new
                        {
                            RoleId = new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                            PermissionId = new Guid("44444444-4444-4444-4444-444444444444")
                        },
                        new
                        {
                            RoleId = new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                            PermissionId = new Guid("55555555-5555-5555-5555-555555555555")
                        },
                        new
                        {
                            RoleId = new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                            PermissionId = new Guid("66666666-6666-6666-6666-666666666666")
                        });
                });

            modelBuilder.Entity("Users.Infrastructure.Models.UserData", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<bool>("Admin")
                        .HasColumnType("boolean");

                    b.Property<DateTime?>("Birthday")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("CreatedBy")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("Gender")
                        .HasColumnType("integer");

                    b.Property<string>("Login")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("ModifiedBy")
                        .HasColumnType("text");

                    b.Property<DateTime?>("ModifiedOn")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("PasswordHash")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("RevokedBy")
                        .HasColumnType("text");

                    b.Property<DateTime?>("RevokedOn")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.ToTable("Users");

                    b.HasData(
                        new
                        {
                            Id = new Guid("99999999-9999-9999-9999-999999999999"),
                            Admin = true,
                            Birthday = new DateTime(1990, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
                            CreatedAt = new DateTime(2025, 5, 17, 0, 0, 0, 0, DateTimeKind.Utc),
                            CreatedBy = "System",
                            Email = "admin@example.com",
                            Gender = 1,
                            Login = "administrator",
                            Name = "Administrator",
                            PasswordHash = "eQ9I47pR4tB2Ln1KdNQHamLPs01E49+8Q3mP6f85lgI="
                        });
                });

            modelBuilder.Entity("Users.Infrastructure.Models.UserRoleEntity", b =>
                {
                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("RoleId")
                        .HasColumnType("uuid");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.ToTable("UserRoles");

                    b.HasData(
                        new
                        {
                            UserId = new Guid("99999999-9999-9999-9999-999999999999"),
                            RoleId = new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa")
                        });
                });

            modelBuilder.Entity("Users.Infrastructure.Models.RolePermissionEntity", b =>
                {
                    b.HasOne("Users.Infrastructure.Models.PermissionEntity", "Permission")
                        .WithMany("RolePermissions")
                        .HasForeignKey("PermissionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Users.Infrastructure.Models.RoleEntity", "Role")
                        .WithMany("RolePermissions")
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Permission");

                    b.Navigation("Role");
                });

            modelBuilder.Entity("Users.Infrastructure.Models.UserRoleEntity", b =>
                {
                    b.HasOne("Users.Infrastructure.Models.RoleEntity", "Role")
                        .WithMany("UserRoles")
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Users.Infrastructure.Models.UserData", "User")
                        .WithMany("UserRoles")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Role");

                    b.Navigation("User");
                });

            modelBuilder.Entity("Users.Infrastructure.Models.PermissionEntity", b =>
                {
                    b.Navigation("RolePermissions");
                });

            modelBuilder.Entity("Users.Infrastructure.Models.RoleEntity", b =>
                {
                    b.Navigation("RolePermissions");

                    b.Navigation("UserRoles");
                });

            modelBuilder.Entity("Users.Infrastructure.Models.UserData", b =>
                {
                    b.Navigation("UserRoles");
                });
#pragma warning restore 612, 618
        }
    }
}
