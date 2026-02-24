using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BaseConLogin.Migrations
{
    /// <inheritdoc />
    public partial class CreacionTablaPropiedadesConfiguradas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductoPropiedades_ProductoConfigurable_ProductoConfigurableProductoBaseId",
                table: "ProductoPropiedades");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductoPropiedades_ProductosBase_ProductoBaseId",
                table: "ProductoPropiedades");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductoPropiedades_PropiedadesMaestras_PropiedadMaestraId",
                table: "ProductoPropiedades");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductoPropiedades",
                table: "ProductoPropiedades");

            migrationBuilder.RenameTable(
                name: "ProductoPropiedades",
                newName: "ProductoPropiedadesConfiguradas");

            migrationBuilder.RenameIndex(
                name: "IX_ProductoPropiedades_PropiedadMaestraId",
                table: "ProductoPropiedadesConfiguradas",
                newName: "IX_ProductoPropiedadesConfiguradas_PropiedadMaestraId");

            migrationBuilder.RenameIndex(
                name: "IX_ProductoPropiedades_ProductoConfigurableProductoBaseId",
                table: "ProductoPropiedadesConfiguradas",
                newName: "IX_ProductoPropiedadesConfiguradas_ProductoConfigurableProductoBaseId");

            migrationBuilder.RenameIndex(
                name: "IX_ProductoPropiedades_ProductoBaseId",
                table: "ProductoPropiedadesConfiguradas",
                newName: "IX_ProductoPropiedadesConfiguradas_ProductoBaseId");

            migrationBuilder.AddColumn<string>(
                name: "NombrePropiedad",
                table: "ProductoPropiedadesConfiguradas",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductoPropiedadesConfiguradas",
                table: "ProductoPropiedadesConfiguradas",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductoPropiedadesConfiguradas_ProductoConfigurable_ProductoConfigurableProductoBaseId",
                table: "ProductoPropiedadesConfiguradas",
                column: "ProductoConfigurableProductoBaseId",
                principalTable: "ProductoConfigurable",
                principalColumn: "ProductoBaseId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductoPropiedadesConfiguradas_ProductosBase_ProductoBaseId",
                table: "ProductoPropiedadesConfiguradas",
                column: "ProductoBaseId",
                principalTable: "ProductosBase",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductoPropiedadesConfiguradas_PropiedadesMaestras_PropiedadMaestraId",
                table: "ProductoPropiedadesConfiguradas",
                column: "PropiedadMaestraId",
                principalTable: "PropiedadesMaestras",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductoPropiedadesConfiguradas_ProductoConfigurable_ProductoConfigurableProductoBaseId",
                table: "ProductoPropiedadesConfiguradas");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductoPropiedadesConfiguradas_ProductosBase_ProductoBaseId",
                table: "ProductoPropiedadesConfiguradas");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductoPropiedadesConfiguradas_PropiedadesMaestras_PropiedadMaestraId",
                table: "ProductoPropiedadesConfiguradas");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductoPropiedadesConfiguradas",
                table: "ProductoPropiedadesConfiguradas");

            migrationBuilder.DropColumn(
                name: "NombrePropiedad",
                table: "ProductoPropiedadesConfiguradas");

            migrationBuilder.RenameTable(
                name: "ProductoPropiedadesConfiguradas",
                newName: "ProductoPropiedades");

            migrationBuilder.RenameIndex(
                name: "IX_ProductoPropiedadesConfiguradas_PropiedadMaestraId",
                table: "ProductoPropiedades",
                newName: "IX_ProductoPropiedades_PropiedadMaestraId");

            migrationBuilder.RenameIndex(
                name: "IX_ProductoPropiedadesConfiguradas_ProductoConfigurableProductoBaseId",
                table: "ProductoPropiedades",
                newName: "IX_ProductoPropiedades_ProductoConfigurableProductoBaseId");

            migrationBuilder.RenameIndex(
                name: "IX_ProductoPropiedadesConfiguradas_ProductoBaseId",
                table: "ProductoPropiedades",
                newName: "IX_ProductoPropiedades_ProductoBaseId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductoPropiedades",
                table: "ProductoPropiedades",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductoPropiedades_ProductoConfigurable_ProductoConfigurableProductoBaseId",
                table: "ProductoPropiedades",
                column: "ProductoConfigurableProductoBaseId",
                principalTable: "ProductoConfigurable",
                principalColumn: "ProductoBaseId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductoPropiedades_ProductosBase_ProductoBaseId",
                table: "ProductoPropiedades",
                column: "ProductoBaseId",
                principalTable: "ProductosBase",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductoPropiedades_PropiedadesMaestras_PropiedadMaestraId",
                table: "ProductoPropiedades",
                column: "PropiedadMaestraId",
                principalTable: "PropiedadesMaestras",
                principalColumn: "Id");
        }
    }
}
