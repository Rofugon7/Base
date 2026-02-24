using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BaseConLogin.Migrations
{
    /// <inheritdoc />
    public partial class AñadirGruposPropiedades : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GrupoPropiedadId",
                table: "ProductoPropiedadesConfiguradas",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "GrupoPropiedades",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TiendaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GrupoPropiedades", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductoPropiedadesConfiguradas_GrupoPropiedadId",
                table: "ProductoPropiedadesConfiguradas",
                column: "GrupoPropiedadId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductoPropiedadesConfiguradas_GrupoPropiedades_GrupoPropiedadId",
                table: "ProductoPropiedadesConfiguradas",
                column: "GrupoPropiedadId",
                principalTable: "GrupoPropiedades",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductoPropiedadesConfiguradas_GrupoPropiedades_GrupoPropiedadId",
                table: "ProductoPropiedadesConfiguradas");

            migrationBuilder.DropTable(
                name: "GrupoPropiedades");

            migrationBuilder.DropIndex(
                name: "IX_ProductoPropiedadesConfiguradas_GrupoPropiedadId",
                table: "ProductoPropiedadesConfiguradas");

            migrationBuilder.DropColumn(
                name: "GrupoPropiedadId",
                table: "ProductoPropiedadesConfiguradas");
        }
    }
}
