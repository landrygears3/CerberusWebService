using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CerberusClassLibrary.Migrations
{
    /// <inheritdoc />
    public partial class AddIsActiveToRefreshToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "UserRefreshTokens",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "UserRefreshTokens");
        }
    }
}
