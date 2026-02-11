using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BaseConLogin.Migrations
{
    /// <inheritdoc />
    public partial class MigTiendaConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TiendaConfigs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NombreEmpresa = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NombreComercial = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CIF = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Direccion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Telefono = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EmailContacto = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    WebURL = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PrefijoFactura = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IvaPorDefecto = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LogoPath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ColorCorporativo = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TiendaConfigs", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TiendaConfigs");
        }
    }
}
