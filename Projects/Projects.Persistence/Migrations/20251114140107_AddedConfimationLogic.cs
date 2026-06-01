using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Projects.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddedConfimationLogic : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ConfirmationInfo",
                table: "Projects",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "ConfirmedAt",
                table: "Projects",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ConfirmedByUserId",
                table: "Projects",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConfirmationInfo",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "ConfirmedAt",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "ConfirmedByUserId",
                table: "Projects");
        }
    }
}
