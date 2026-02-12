using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BaseConLogin.Migrations
{
    /// <inheritdoc />
    public partial class AddImpresionesYConfiguracion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FormatosPermitidos",
                table: "TiendaConfigs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "MaxFileSizeMB",
                table: "TiendaConfigs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "TrabajosImpresion",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UsuarioId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    NombreArchivoOriginal = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NombreArchivoServidor = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RutaFisica = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NifLimpio = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaSubida = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Notas = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ArchivoEliminado = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrabajosImpresion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrabajosImpresion_AspNetUsers_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_TrabajosImpresion_UsuarioId",
                table: "TrabajosImpresion",
                column: "UsuarioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TrabajosImpresion");

            migrationBuilder.DropColumn(
                name: "FormatosPermitidos",
                table: "TiendaConfigs");

            migrationBuilder.DropColumn(
                name: "MaxFileSizeMB",
                table: "TiendaConfigs");
        }
    }
}
