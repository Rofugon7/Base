using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BaseConLogin.Migrations
{
    /// <inheritdoc />
    public partial class AñadirGruposPropiedades2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GrupoPropiedadId",
                table: "PropiedadesGenericas",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PropiedadesGenericas_GrupoPropiedadId",
                table: "PropiedadesGenericas",
                column: "GrupoPropiedadId");

            migrationBuilder.AddForeignKey(
                name: "FK_PropiedadesGenericas_GrupoPropiedades_GrupoPropiedadId",
                table: "PropiedadesGenericas",
                column: "GrupoPropiedadId",
                principalTable: "GrupoPropiedades",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PropiedadesGenericas_GrupoPropiedades_GrupoPropiedadId",
                table: "PropiedadesGenericas");

            migrationBuilder.DropIndex(
                name: "IX_PropiedadesGenericas_GrupoPropiedadId",
                table: "PropiedadesGenericas");

            migrationBuilder.DropColumn(
                name: "GrupoPropiedadId",
                table: "PropiedadesGenericas");
        }
    }
}
