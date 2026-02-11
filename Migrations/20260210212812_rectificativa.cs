using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BaseConLogin.Migrations
{
    /// <inheritdoc />
    public partial class rectificativa : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "EsRectificativa",
                table: "Facturas",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "FacturaOriginalId",
                table: "Facturas",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HashActual",
                table: "Facturas",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "HashAnterior",
                table: "Facturas",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "MotivoRectificacion",
                table: "Facturas",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Facturas_FacturaOriginalId",
                table: "Facturas",
                column: "FacturaOriginalId");

            migrationBuilder.AddForeignKey(
                name: "FK_Facturas_Facturas_FacturaOriginalId",
                table: "Facturas",
                column: "FacturaOriginalId",
                principalTable: "Facturas",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Facturas_Facturas_FacturaOriginalId",
                table: "Facturas");

            migrationBuilder.DropIndex(
                name: "IX_Facturas_FacturaOriginalId",
                table: "Facturas");

            migrationBuilder.DropColumn(
                name: "EsRectificativa",
                table: "Facturas");

            migrationBuilder.DropColumn(
                name: "FacturaOriginalId",
                table: "Facturas");

            migrationBuilder.DropColumn(
                name: "HashActual",
                table: "Facturas");

            migrationBuilder.DropColumn(
                name: "HashAnterior",
                table: "Facturas");

            migrationBuilder.DropColumn(
                name: "MotivoRectificacion",
                table: "Facturas");
        }
    }
}
