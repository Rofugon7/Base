using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BaseConLogin.Migrations
{
    /// <inheritdoc />
    public partial class MigTiendaConfigAmpiacion5 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MapaIncrustado",
                table: "TiendaConfigs",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MapaIncrustado",
                table: "TiendaConfigs");
        }
    }
}
