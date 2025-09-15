using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BaseConLogin.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRevisadoToProyectos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Revisado",
                table: "Proyectos",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Revisado",
                table: "Proyectos");
        }
    }
}
