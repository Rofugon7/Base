using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BaseConLogin.Migrations
{
    /// <inheritdoc />
    public partial class AddPresupuesto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Presupuestos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Cantidad = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DimensionPlana = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DimensionFinal = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CarasImpresas = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TipoPapel = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Acabado = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InformacionAdicional = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Apellidos = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Telefono = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaSolicitud = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Presupuestos", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Presupuestos");
        }
    }
}
