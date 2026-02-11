using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BaseConLogin.Migrations
{
    /// <inheritdoc />
    public partial class MigTiendaConfigAmpiacion3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EmailAdministrador",
                table: "TiendaConfigs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmailAdministrador",
                table: "TiendaConfigs");
        }
    }
}
