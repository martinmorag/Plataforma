using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Plataforma.Migrations
{
    /// <inheritdoc />
    public partial class actualizarcurso : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Disponible",
                table: "cursos",
                newName: "Habilitado");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Habilitado",
                table: "cursos",
                newName: "Disponible");
        }
    }
}
