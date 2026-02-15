using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BaseConLogin.Migrations
{
    /// <inheritdoc />
    public partial class TP : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "EsConfigurablePorUsuario",
                table: "PropiedadesMaestras",
                newName: "EsConfigurablePorDefecto");

            migrationBuilder.AlterColumn<decimal>(
                name: "PrecioBase",
                table: "ProductosBase",
                type: "decimal(18,4)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AddColumn<int>(
                name: "TipoProducto",
                table: "ProductosBase",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TipoProducto",
                table: "ProductosBase");

            migrationBuilder.RenameColumn(
                name: "EsConfigurablePorDefecto",
                table: "PropiedadesMaestras",
                newName: "EsConfigurablePorUsuario");

            migrationBuilder.AlterColumn<decimal>(
                name: "PrecioBase",
                table: "ProductosBase",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)",
                oldPrecision: 18,
                oldScale: 2);
        }
    }
}
