using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Projects.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class fixedCurrencyCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Currency",
                table: "Projects",
                newName: "CurrencyCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CurrencyCode",
                table: "Projects",
                newName: "Currency");
        }
    }
}
