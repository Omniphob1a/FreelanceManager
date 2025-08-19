using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Projects.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixedMilestones : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsEscalated",
                table: "ProjectMilestones",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsEscalated",
                table: "ProjectMilestones");
        }
    }
}
