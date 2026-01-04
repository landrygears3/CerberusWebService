using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CerberusClassLibrary.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUrlFisicaFromNavigationConfig_v2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "URLFisica",
                table: "NavigationConfigs");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "URLFisica",
                table: "NavigationConfigs",
                type: "nvarchar(2048)",
                maxLength: 2048,
                nullable: true);
        }
    }
}
