using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BaseConLogin.Migrations
{
    /// <inheritdoc />
    public partial class MigTiendaConfigAmpiacion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "WebURL",
                table: "TiendaConfigs",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "NombreComercial",
                table: "TiendaConfigs",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "LogoPath",
                table: "TiendaConfigs",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "Actividad",
                table: "TiendaConfigs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Folio",
                table: "TiendaConfigs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Hoja",
                table: "TiendaConfigs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Inscripcion",
                table: "TiendaConfigs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Maps",
                table: "TiendaConfigs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RegistroMercantil",
                table: "TiendaConfigs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Tomo",
                table: "TiendaConfigs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UrlFacebook",
                table: "TiendaConfigs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UrlInstagram",
                table: "TiendaConfigs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UrlTikTok",
                table: "TiendaConfigs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UrlX",
                table: "TiendaConfigs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Whatsapp",
                table: "TiendaConfigs",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Actividad",
                table: "TiendaConfigs");

            migrationBuilder.DropColumn(
                name: "Folio",
                table: "TiendaConfigs");

            migrationBuilder.DropColumn(
                name: "Hoja",
                table: "TiendaConfigs");

            migrationBuilder.DropColumn(
                name: "Inscripcion",
                table: "TiendaConfigs");

            migrationBuilder.DropColumn(
                name: "Maps",
                table: "TiendaConfigs");

            migrationBuilder.DropColumn(
                name: "RegistroMercantil",
                table: "TiendaConfigs");

            migrationBuilder.DropColumn(
                name: "Tomo",
                table: "TiendaConfigs");

            migrationBuilder.DropColumn(
                name: "UrlFacebook",
                table: "TiendaConfigs");

            migrationBuilder.DropColumn(
                name: "UrlInstagram",
                table: "TiendaConfigs");

            migrationBuilder.DropColumn(
                name: "UrlTikTok",
                table: "TiendaConfigs");

            migrationBuilder.DropColumn(
                name: "UrlX",
                table: "TiendaConfigs");

            migrationBuilder.DropColumn(
                name: "Whatsapp",
                table: "TiendaConfigs");

            migrationBuilder.AlterColumn<string>(
                name: "WebURL",
                table: "TiendaConfigs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "NombreComercial",
                table: "TiendaConfigs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "LogoPath",
                table: "TiendaConfigs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}
