using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Plataforma.Migrations
{
    /// <inheritdoc />
    public partial class changeentrega : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tareas_clases_ClaseId1",
                table: "tareas");

            migrationBuilder.DropIndex(
                name: "IX_tareas_ClaseId1",
                table: "tareas");

            migrationBuilder.DropColumn(
                name: "ClaseId1",
                table: "tareas");

            migrationBuilder.AlterColumn<DateTime>(
                name: "FechaEntrega",
                table: "entregas",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AddColumn<TimeSpan>(
                name: "ProgresoVideo",
                table: "entregas",
                type: "interval",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProgresoVideo",
                table: "entregas");

            migrationBuilder.AddColumn<Guid>(
                name: "ClaseId1",
                table: "tareas",
                type: "uuid",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "FechaEntrega",
                table: "entregas",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_tareas_ClaseId1",
                table: "tareas",
                column: "ClaseId1");

            migrationBuilder.AddForeignKey(
                name: "FK_tareas_clases_ClaseId1",
                table: "tareas",
                column: "ClaseId1",
                principalTable: "clases",
                principalColumn: "ClaseId");
        }
    }
}
