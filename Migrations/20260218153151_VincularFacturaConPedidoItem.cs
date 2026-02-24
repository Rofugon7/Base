using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BaseConLogin.Migrations
{
    /// <inheritdoc />
    public partial class VincularFacturaConPedidoItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Grupo",
                table: "PropiedadesGenericas",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Tipo",
                table: "PropiedadesGenericas",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PedidoItemId",
                table: "FacturaLineas",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DetallesOpciones",
                table: "CarritoItems",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PrecioPersonalizado",
                table: "CarritoItems",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "PedidoDetallePropiedades",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PedidoDetalleId = table.Column<int>(type: "int", nullable: false),
                    NombrePropiedad = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ValorSeleccionado = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    PrecioAplicado = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PedidoDetallePropiedades", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PedidoDetallePropiedades_PedidoItems_PedidoDetalleId",
                        column: x => x.PedidoDetalleId,
                        principalTable: "PedidoItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FacturaLineas_PedidoItemId",
                table: "FacturaLineas",
                column: "PedidoItemId");

            migrationBuilder.CreateIndex(
                name: "IX_PedidoDetallePropiedades_PedidoDetalleId",
                table: "PedidoDetallePropiedades",
                column: "PedidoDetalleId");

            migrationBuilder.AddForeignKey(
                name: "FK_FacturaLineas_PedidoItems_PedidoItemId",
                table: "FacturaLineas",
                column: "PedidoItemId",
                principalTable: "PedidoItems",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FacturaLineas_PedidoItems_PedidoItemId",
                table: "FacturaLineas");

            migrationBuilder.DropTable(
                name: "PedidoDetallePropiedades");

            migrationBuilder.DropIndex(
                name: "IX_FacturaLineas_PedidoItemId",
                table: "FacturaLineas");

            migrationBuilder.DropColumn(
                name: "Grupo",
                table: "PropiedadesGenericas");

            migrationBuilder.DropColumn(
                name: "Tipo",
                table: "PropiedadesGenericas");

            migrationBuilder.DropColumn(
                name: "PedidoItemId",
                table: "FacturaLineas");

            migrationBuilder.DropColumn(
                name: "DetallesOpciones",
                table: "CarritoItems");

            migrationBuilder.DropColumn(
                name: "PrecioPersonalizado",
                table: "CarritoItems");
        }
    }
}
