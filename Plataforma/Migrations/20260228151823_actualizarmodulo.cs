using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Plataforma.Migrations
{
    /// <inheritdoc />
    public partial class actualizarmodulo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Titulo",
                table: "modulos",
                newName: "Nombre");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Nombre",
                table: "modulos",
                newName: "Titulo");
        }
    }
}
