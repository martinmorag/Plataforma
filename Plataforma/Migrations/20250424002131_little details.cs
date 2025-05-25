using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Plataforma.Migrations
{
    /// <inheritdoc />
    public partial class littledetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ProfesorId",
                table: "cursos",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "tareas",
                columns: table => new
                {
                    TareaId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ClaseId = table.Column<Guid>(type: "uuid", nullable: false),
                    EstudianteId = table.Column<Guid>(type: "uuid", nullable: false),
                    ArchivoUrl = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    FileName = table.Column<string>(type: "text", nullable: false),
                    FechaEntrega = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ContentType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Descripcion = table.Column<string>(type: "text", nullable: false),
                    RespuestaEstudianteArchivoUrl = table.Column<string>(type: "text", nullable: false),
                    RespuestaEstudianteFileName = table.Column<string>(type: "text", nullable: false),
                    RespuestaEstudianteContentType = table.Column<string>(type: "text", nullable: false),
                    RespuestaEstudianteFechaEntrega = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tareas", x => x.TareaId);
                    table.ForeignKey(
                        name: "FK_tareas_AspNetUsers_EstudianteId",
                        column: x => x.EstudianteId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tareas_clases_ClaseId",
                        column: x => x.ClaseId,
                        principalTable: "clases",
                        principalColumn: "ClaseId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_cursos_ProfesorId",
                table: "cursos",
                column: "ProfesorId");

            migrationBuilder.CreateIndex(
                name: "IX_tareas_ClaseId",
                table: "tareas",
                column: "ClaseId");

            migrationBuilder.CreateIndex(
                name: "IX_tareas_EstudianteId",
                table: "tareas",
                column: "EstudianteId");

            migrationBuilder.AddForeignKey(
                name: "FK_cursos_AspNetUsers_ProfesorId",
                table: "cursos",
                column: "ProfesorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_cursos_AspNetUsers_ProfesorId",
                table: "cursos");

            migrationBuilder.DropTable(
                name: "tareas");

            migrationBuilder.DropIndex(
                name: "IX_cursos_ProfesorId",
                table: "cursos");

            migrationBuilder.DropColumn(
                name: "ProfesorId",
                table: "cursos");
        }
    }
}
