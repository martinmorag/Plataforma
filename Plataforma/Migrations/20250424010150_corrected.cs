using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Plataforma.Migrations
{
    /// <inheritdoc />
    public partial class corrected : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_cursos_AspNetUsers_EstudianteId",
                table: "cursos");

            migrationBuilder.DropForeignKey(
                name: "FK_cursos_AspNetUsers_ProfesorId",
                table: "cursos");

            migrationBuilder.DropIndex(
                name: "IX_cursos_EstudianteId",
                table: "cursos");

            migrationBuilder.DropIndex(
                name: "IX_cursos_ProfesorId",
                table: "cursos");

            migrationBuilder.DropColumn(
                name: "EstudianteId",
                table: "cursos");

            migrationBuilder.DropColumn(
                name: "ProfesorId",
                table: "cursos");

            migrationBuilder.CreateTable(
                name: "CursoEstudiantes",
                columns: table => new
                {
                    CursoId = table.Column<Guid>(type: "uuid", nullable: false),
                    EstudianteId = table.Column<Guid>(type: "uuid", nullable: false),
                    CursoEstudianteId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CursoEstudiantes", x => new { x.CursoId, x.EstudianteId });
                    table.ForeignKey(
                        name: "FK_CursoEstudiantes_AspNetUsers_EstudianteId",
                        column: x => x.EstudianteId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CursoEstudiantes_cursos_CursoId",
                        column: x => x.CursoId,
                        principalTable: "cursos",
                        principalColumn: "CursoId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CursoProfesores",
                columns: table => new
                {
                    CursoId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProfesorId = table.Column<Guid>(type: "uuid", nullable: false),
                    CursoProfesorId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CursoProfesores", x => new { x.CursoId, x.ProfesorId });
                    table.ForeignKey(
                        name: "FK_CursoProfesores_AspNetUsers_ProfesorId",
                        column: x => x.ProfesorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CursoProfesores_cursos_CursoId",
                        column: x => x.CursoId,
                        principalTable: "cursos",
                        principalColumn: "CursoId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CursoEstudiantes_EstudianteId",
                table: "CursoEstudiantes",
                column: "EstudianteId");

            migrationBuilder.CreateIndex(
                name: "IX_CursoProfesores_ProfesorId",
                table: "CursoProfesores",
                column: "ProfesorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CursoEstudiantes");

            migrationBuilder.DropTable(
                name: "CursoProfesores");

            migrationBuilder.AddColumn<Guid>(
                name: "EstudianteId",
                table: "cursos",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "ProfesorId",
                table: "cursos",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_cursos_EstudianteId",
                table: "cursos",
                column: "EstudianteId");

            migrationBuilder.CreateIndex(
                name: "IX_cursos_ProfesorId",
                table: "cursos",
                column: "ProfesorId");

            migrationBuilder.AddForeignKey(
                name: "FK_cursos_AspNetUsers_EstudianteId",
                table: "cursos",
                column: "EstudianteId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_cursos_AspNetUsers_ProfesorId",
                table: "cursos",
                column: "ProfesorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
