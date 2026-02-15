using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BaseConLogin.Migrations
{
    /// <inheritdoc />
    public partial class EliminarProductoSimpleYUnificarStock : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductosConfigurables_ProductosBase_ProductoBaseId",
                table: "ProductosConfigurables");

            migrationBuilder.DropTable(
                name: "productosSimples");

            migrationBuilder.DropIndex(
                name: "IX_Proyectos_ProductoBaseId",
                table: "Proyectos");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductosConfigurables",
                table: "ProductosConfigurables");

            migrationBuilder.RenameTable(
                name: "ProductosConfigurables",
                newName: "ProductoConfigurable");

            migrationBuilder.RenameColumn(
                name: "TipoProducto",
                table: "ProductosBase",
                newName: "Stock");

            migrationBuilder.AddColumn<int>(
                name: "Stock",
                table: "ProductoConfigurable",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductoConfigurable",
                table: "ProductoConfigurable",
                column: "ProductoBaseId");

            migrationBuilder.CreateTable(
                name: "PropiedadesMaestras",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TiendaId = table.Column<int>(type: "int", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EsConfigurablePorUsuario = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PropiedadesMaestras", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PropiedadesMaestras_Tiendas_TiendaId",
                        column: x => x.TiendaId,
                        principalTable: "Tiendas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductoPropiedades",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductoBaseId = table.Column<int>(type: "int", nullable: false),
                    PropiedadMaestraId = table.Column<int>(type: "int", nullable: true),
                    NombreEnProducto = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Valor = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Operacion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Orden = table.Column<int>(type: "int", nullable: false),
                    EsConfigurable = table.Column<bool>(type: "bit", nullable: false),
                    ProductoConfigurableProductoBaseId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductoPropiedades", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductoPropiedades_ProductoConfigurable_ProductoConfigurableProductoBaseId",
                        column: x => x.ProductoConfigurableProductoBaseId,
                        principalTable: "ProductoConfigurable",
                        principalColumn: "ProductoBaseId");
                    table.ForeignKey(
                        name: "FK_ProductoPropiedades_ProductosBase_ProductoBaseId",
                        column: x => x.ProductoBaseId,
                        principalTable: "ProductosBase",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductoPropiedades_PropiedadesMaestras_PropiedadMaestraId",
                        column: x => x.PropiedadMaestraId,
                        principalTable: "PropiedadesMaestras",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Proyectos_ProductoBaseId",
                table: "Proyectos",
                column: "ProductoBaseId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductoPropiedades_ProductoBaseId",
                table: "ProductoPropiedades",
                column: "ProductoBaseId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductoPropiedades_ProductoConfigurableProductoBaseId",
                table: "ProductoPropiedades",
                column: "ProductoConfigurableProductoBaseId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductoPropiedades_PropiedadMaestraId",
                table: "ProductoPropiedades",
                column: "PropiedadMaestraId");

            migrationBuilder.CreateIndex(
                name: "IX_PropiedadesMaestras_TiendaId",
                table: "PropiedadesMaestras",
                column: "TiendaId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductoConfigurable_ProductosBase_ProductoBaseId",
                table: "ProductoConfigurable",
                column: "ProductoBaseId",
                principalTable: "ProductosBase",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductoConfigurable_ProductosBase_ProductoBaseId",
                table: "ProductoConfigurable");

            migrationBuilder.DropTable(
                name: "ProductoPropiedades");

            migrationBuilder.DropTable(
                name: "PropiedadesMaestras");

            migrationBuilder.DropIndex(
                name: "IX_Proyectos_ProductoBaseId",
                table: "Proyectos");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductoConfigurable",
                table: "ProductoConfigurable");

            migrationBuilder.DropColumn(
                name: "Stock",
                table: "ProductoConfigurable");

            migrationBuilder.RenameTable(
                name: "ProductoConfigurable",
                newName: "ProductosConfigurables");

            migrationBuilder.RenameColumn(
                name: "Stock",
                table: "ProductosBase",
                newName: "TipoProducto");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductosConfigurables",
                table: "ProductosConfigurables",
                column: "ProductoBaseId");

            migrationBuilder.CreateTable(
                name: "productosSimples",
                columns: table => new
                {
                    ProductoBaseId = table.Column<int>(type: "int", nullable: false),
                    Stock = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_productosSimples", x => x.ProductoBaseId);
                    table.ForeignKey(
                        name: "FK_productosSimples_ProductosBase_ProductoBaseId",
                        column: x => x.ProductoBaseId,
                        principalTable: "ProductosBase",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Proyectos_ProductoBaseId",
                table: "Proyectos",
                column: "ProductoBaseId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductosConfigurables_ProductosBase_ProductoBaseId",
                table: "ProductosConfigurables",
                column: "ProductoBaseId",
                principalTable: "ProductosBase",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
