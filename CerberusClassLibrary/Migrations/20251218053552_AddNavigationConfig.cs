using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CerberusClassLibrary.Migrations
{
    /// <inheritdoc />
    public partial class AddNavigationConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NavigationConfigs",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    titulo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    url = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    url_slug = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    id_padre = table.Column<int>(type: "int", nullable: true),
                    orden = table.Column<int>(type: "int", nullable: false),
                    icono = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    descripcion = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    activo = table.Column<bool>(type: "bit", nullable: false),
                    abrir_nueva_ventana = table.Column<bool>(type: "bit", nullable: false),
                    nivel = table.Column<int>(type: "int", nullable: false),
                    URLFisica = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NavigationConfigs", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NavigationConfigs");
        }
    }
}
