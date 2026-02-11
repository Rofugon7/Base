using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BaseConLogin.Migrations
{
    /// <inheritdoc />
    public partial class MigTiendaConfigAmpiacion7 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ColorBotones",
                table: "TiendaConfigs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ColorBotonesTexto",
                table: "TiendaConfigs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ColorEnlaces",
                table: "TiendaConfigs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ColorTextos",
                table: "TiendaConfigs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ColorBotones",
                table: "TiendaConfigs");

            migrationBuilder.DropColumn(
                name: "ColorBotonesTexto",
                table: "TiendaConfigs");

            migrationBuilder.DropColumn(
                name: "ColorEnlaces",
                table: "TiendaConfigs");

            migrationBuilder.DropColumn(
                name: "ColorTextos",
                table: "TiendaConfigs");
        }
    }
}
