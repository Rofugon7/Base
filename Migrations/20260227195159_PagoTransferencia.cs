using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BaseConLogin.Migrations
{
    /// <inheritdoc />
    public partial class PagoTransferencia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IbanTransferencia",
                table: "TiendaConfigs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NombreBanco",
                table: "TiendaConfigs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TitularCuenta",
                table: "TiendaConfigs",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IbanTransferencia",
                table: "TiendaConfigs");

            migrationBuilder.DropColumn(
                name: "NombreBanco",
                table: "TiendaConfigs");

            migrationBuilder.DropColumn(
                name: "TitularCuenta",
                table: "TiendaConfigs");
        }
    }
}
