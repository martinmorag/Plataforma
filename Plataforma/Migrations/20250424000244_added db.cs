using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Plataforma.Migrations
{
    /// <inheritdoc />
    public partial class addeddb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "AspNetUsers",
                type: "character varying(21)",
                maxLength: 21,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "cursos",
                columns: table => new
                {
                    CursoId = table.Column<Guid>(type: "uuid", nullable: false),
                    Nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    EstudianteId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cursos", x => x.CursoId);
                    table.ForeignKey(
                        name: "FK_cursos_AspNetUsers_EstudianteId",
                        column: x => x.EstudianteId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "modulos",
                columns: table => new
                {
                    ModuloId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CursoId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_modulos", x => x.ModuloId);
                    table.ForeignKey(
                        name: "FK_modulos_cursos_CursoId",
                        column: x => x.CursoId,
                        principalTable: "cursos",
                        principalColumn: "CursoId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "clases",
                columns: table => new
                {
                    ClaseId = table.Column<Guid>(type: "uuid", nullable: false),
                    Nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ModuloId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_clases", x => x.ClaseId);
                    table.ForeignKey(
                        name: "FK_clases_modulos_ModuloId",
                        column: x => x.ModuloId,
                        principalTable: "modulos",
                        principalColumn: "ModuloId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_clases_ModuloId",
                table: "clases",
                column: "ModuloId");

            migrationBuilder.CreateIndex(
                name: "IX_cursos_EstudianteId",
                table: "cursos",
                column: "EstudianteId");

            migrationBuilder.CreateIndex(
                name: "IX_modulos_CursoId",
                table: "modulos",
                column: "CursoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "clases");

            migrationBuilder.DropTable(
                name: "modulos");

            migrationBuilder.DropTable(
                name: "cursos");

            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "AspNetUsers");
        }
    }
}
