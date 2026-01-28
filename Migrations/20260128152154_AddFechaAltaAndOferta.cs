using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BaseConLogin.Migrations
{
    /// <inheritdoc />
    public partial class AddFechaAltaAndOferta : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductosSimples_ProductosBase_ProductoBaseId",
                table: "ProductosSimples");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductosSimples",
                table: "ProductosSimples");

            migrationBuilder.RenameTable(
                name: "ProductosSimples",
                newName: "productosSimples");

            migrationBuilder.AddPrimaryKey(
                name: "PK_productosSimples",
                table: "productosSimples",
                column: "ProductoBaseId");

            migrationBuilder.AddForeignKey(
                name: "FK_productosSimples_ProductosBase_ProductoBaseId",
                table: "productosSimples",
                column: "ProductoBaseId",
                principalTable: "ProductosBase",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_productosSimples_ProductosBase_ProductoBaseId",
                table: "productosSimples");

            migrationBuilder.DropPrimaryKey(
                name: "PK_productosSimples",
                table: "productosSimples");

            migrationBuilder.RenameTable(
                name: "productosSimples",
                newName: "ProductosSimples");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductosSimples",
                table: "ProductosSimples",
                column: "ProductoBaseId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductosSimples_ProductosBase_ProductoBaseId",
                table: "ProductosSimples",
                column: "ProductoBaseId",
                principalTable: "ProductosBase",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
