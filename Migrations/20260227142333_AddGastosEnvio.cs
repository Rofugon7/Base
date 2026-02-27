using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BaseConLogin.Migrations
{
    /// <inheritdoc />
    public partial class AddGastosEnvio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "EnvioEstandar",
                table: "TiendaConfigs",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "EnvioGratisDesde",
                table: "TiendaConfigs",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "EnvioUrgente",
                table: "TiendaConfigs",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "PermitirRecogidaTienda",
                table: "TiendaConfigs",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EnvioEstandar",
                table: "TiendaConfigs");

            migrationBuilder.DropColumn(
                name: "EnvioGratisDesde",
                table: "TiendaConfigs");

            migrationBuilder.DropColumn(
                name: "EnvioUrgente",
                table: "TiendaConfigs");

            migrationBuilder.DropColumn(
                name: "PermitirRecogidaTienda",
                table: "TiendaConfigs");
        }
    }
}
