using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BaseConLogin.Migrations
{
    /// <inheritdoc />
    public partial class AddSlugToProjectsAndTags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Nombre",
                table: "CarritoItems");

            migrationBuilder.DropColumn(
                name: "PrecioUnitario",
                table: "CarritoItems");

            migrationBuilder.DropColumn(
                name: "TipoProducto",
                table: "CarritoItems");

            migrationBuilder.AddColumn<string>(
                name: "Slug",
                table: "Tags",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Slug",
                table: "Proyectos",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Carritos_TiendaId",
                table: "Carritos",
                column: "TiendaId");

            migrationBuilder.CreateIndex(
                name: "IX_CarritoItems_ProductoBaseId",
                table: "CarritoItems",
                column: "ProductoBaseId");

            migrationBuilder.AddForeignKey(
                name: "FK_CarritoItems_ProductosBase_ProductoBaseId",
                table: "CarritoItems",
                column: "ProductoBaseId",
                principalTable: "ProductosBase",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Carritos_Tiendas_TiendaId",
                table: "Carritos",
                column: "TiendaId",
                principalTable: "Tiendas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CarritoItems_ProductosBase_ProductoBaseId",
                table: "CarritoItems");

            migrationBuilder.DropForeignKey(
                name: "FK_Carritos_Tiendas_TiendaId",
                table: "Carritos");

            migrationBuilder.DropIndex(
                name: "IX_Carritos_TiendaId",
                table: "Carritos");

            migrationBuilder.DropIndex(
                name: "IX_CarritoItems_ProductoBaseId",
                table: "CarritoItems");

            migrationBuilder.DropColumn(
                name: "Slug",
                table: "Tags");

            migrationBuilder.DropColumn(
                name: "Slug",
                table: "Proyectos");

            migrationBuilder.AddColumn<string>(
                name: "Nombre",
                table: "CarritoItems",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "PrecioUnitario",
                table: "CarritoItems",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "TipoProducto",
                table: "CarritoItems",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
