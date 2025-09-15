using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BaseConLogin.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddFieldsToProject : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Adelanto",
                table: "Proyectos",
                type: "nvarchar(600)",
                maxLength: 600,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Autor",
                table: "Proyectos",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Calendario",
                table: "Proyectos",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Categoria",
                table: "Proyectos",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Recompensa",
                table: "Proyectos",
                type: "nvarchar(600)",
                maxLength: 600,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Subtitulo",
                table: "Proyectos",
                type: "nvarchar(350)",
                maxLength: 350,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Ubicacion",
                table: "Proyectos",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Adelanto",
                table: "Proyectos");

            migrationBuilder.DropColumn(
                name: "Autor",
                table: "Proyectos");

            migrationBuilder.DropColumn(
                name: "Calendario",
                table: "Proyectos");

            migrationBuilder.DropColumn(
                name: "Categoria",
                table: "Proyectos");

            migrationBuilder.DropColumn(
                name: "Recompensa",
                table: "Proyectos");

            migrationBuilder.DropColumn(
                name: "Subtitulo",
                table: "Proyectos");

            migrationBuilder.DropColumn(
                name: "Ubicacion",
                table: "Proyectos");
        }
    }
}
