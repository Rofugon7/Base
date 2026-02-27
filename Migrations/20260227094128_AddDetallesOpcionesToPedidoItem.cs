using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BaseConLogin.Migrations
{
    /// <inheritdoc />
    public partial class AddDetallesOpcionesToPedidoItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "GastosEnvio",
                table: "Pedidos",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "OpcionesSeleccionadas",
                table: "PedidoItems",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "GastosEnvio",
                table: "Carritos",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GastosEnvio",
                table: "Pedidos");

            migrationBuilder.DropColumn(
                name: "OpcionesSeleccionadas",
                table: "PedidoItems");

            migrationBuilder.DropColumn(
                name: "GastosEnvio",
                table: "Carritos");
        }
    }
}
