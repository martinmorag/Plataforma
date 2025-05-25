using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Plataforma.Migrations
{
    /// <inheritdoc />
    public partial class changetareas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tareas_AspNetUsers_EstudianteId",
                table: "tareas");

            migrationBuilder.DropForeignKey(
                name: "FK_tareas_clases_ClaseId",
                table: "tareas");

            migrationBuilder.DropIndex(
                name: "IX_tareas_EstudianteId",
                table: "tareas");

            migrationBuilder.DropColumn(
                name: "ArchivoUrl",
                table: "tareas");

            migrationBuilder.DropColumn(
                name: "ContentType",
                table: "tareas");

            migrationBuilder.DropColumn(
                name: "EstudianteId",
                table: "tareas");

            migrationBuilder.DropColumn(
                name: "FileName",
                table: "tareas");

            migrationBuilder.DropColumn(
                name: "RespuestaEstudianteArchivoUrl",
                table: "tareas");

            migrationBuilder.DropColumn(
                name: "RespuestaEstudianteContentType",
                table: "tareas");

            migrationBuilder.DropColumn(
                name: "RespuestaEstudianteFechaEntrega",
                table: "tareas");

            migrationBuilder.DropColumn(
                name: "RespuestaEstudianteFileName",
                table: "tareas");

            migrationBuilder.RenameColumn(
                name: "FechaEntrega",
                table: "tareas",
                newName: "FechaVencimiento");

            migrationBuilder.AddColumn<Guid>(
                name: "ArchivoId",
                table: "tareas",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ClaseId1",
                table: "tareas",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TipoEntregaEsperado",
                table: "tareas",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "archivos",
                columns: table => new
                {
                    ArchivoId = table.Column<Guid>(type: "uuid", nullable: false),
                    ArchivoUrl = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    FileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SizeInBytes = table.Column<long>(type: "bigint", nullable: false),
                    DuracionVideo = table.Column<TimeSpan>(type: "interval", nullable: true),
                    FechaSubida = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_archivos", x => x.ArchivoId);
                });

            migrationBuilder.CreateTable(
                name: "entregas",
                columns: table => new
                {
                    EntregaId = table.Column<Guid>(type: "uuid", nullable: false),
                    TareaId = table.Column<Guid>(type: "uuid", nullable: false),
                    EstudianteId = table.Column<Guid>(type: "uuid", nullable: false),
                    Estado = table.Column<int>(type: "integer", nullable: false),
                    FechaEntrega = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ComentariosProfesor = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ArchivoId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_entregas", x => x.EntregaId);
                    table.ForeignKey(
                        name: "FK_entregas_AspNetUsers_EstudianteId",
                        column: x => x.EstudianteId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_entregas_archivos_ArchivoId",
                        column: x => x.ArchivoId,
                        principalTable: "archivos",
                        principalColumn: "ArchivoId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_entregas_tareas_TareaId",
                        column: x => x.TareaId,
                        principalTable: "tareas",
                        principalColumn: "TareaId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tareas_ArchivoId",
                table: "tareas",
                column: "ArchivoId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tareas_ClaseId1",
                table: "tareas",
                column: "ClaseId1");

            migrationBuilder.CreateIndex(
                name: "IX_entregas_ArchivoId",
                table: "entregas",
                column: "ArchivoId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_entregas_EstudianteId",
                table: "entregas",
                column: "EstudianteId");

            migrationBuilder.CreateIndex(
                name: "IX_entregas_TareaId",
                table: "entregas",
                column: "TareaId");

            migrationBuilder.AddForeignKey(
                name: "FK_tareas_archivos_ArchivoId",
                table: "tareas",
                column: "ArchivoId",
                principalTable: "archivos",
                principalColumn: "ArchivoId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_tareas_clases_ClaseId",
                table: "tareas",
                column: "ClaseId",
                principalTable: "clases",
                principalColumn: "ClaseId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_tareas_clases_ClaseId1",
                table: "tareas",
                column: "ClaseId1",
                principalTable: "clases",
                principalColumn: "ClaseId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tareas_archivos_ArchivoId",
                table: "tareas");

            migrationBuilder.DropForeignKey(
                name: "FK_tareas_clases_ClaseId",
                table: "tareas");

            migrationBuilder.DropForeignKey(
                name: "FK_tareas_clases_ClaseId1",
                table: "tareas");

            migrationBuilder.DropTable(
                name: "entregas");

            migrationBuilder.DropTable(
                name: "archivos");

            migrationBuilder.DropIndex(
                name: "IX_tareas_ArchivoId",
                table: "tareas");

            migrationBuilder.DropIndex(
                name: "IX_tareas_ClaseId1",
                table: "tareas");

            migrationBuilder.DropColumn(
                name: "ArchivoId",
                table: "tareas");

            migrationBuilder.DropColumn(
                name: "ClaseId1",
                table: "tareas");

            migrationBuilder.DropColumn(
                name: "TipoEntregaEsperado",
                table: "tareas");

            migrationBuilder.RenameColumn(
                name: "FechaVencimiento",
                table: "tareas",
                newName: "FechaEntrega");

            migrationBuilder.AddColumn<string>(
                name: "ArchivoUrl",
                table: "tareas",
                type: "character varying(2048)",
                maxLength: 2048,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ContentType",
                table: "tareas",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "EstudianteId",
                table: "tareas",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "FileName",
                table: "tareas",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RespuestaEstudianteArchivoUrl",
                table: "tareas",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RespuestaEstudianteContentType",
                table: "tareas",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RespuestaEstudianteFechaEntrega",
                table: "tareas",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RespuestaEstudianteFileName",
                table: "tareas",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_tareas_EstudianteId",
                table: "tareas",
                column: "EstudianteId");

            migrationBuilder.AddForeignKey(
                name: "FK_tareas_AspNetUsers_EstudianteId",
                table: "tareas",
                column: "EstudianteId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_tareas_clases_ClaseId",
                table: "tareas",
                column: "ClaseId",
                principalTable: "clases",
                principalColumn: "ClaseId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
