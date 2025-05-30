using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Plataforma.Migrations
{
    /// <inheritdoc />
    public partial class _2wqwe : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "TareaId1",
                table: "entregas",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_entregas_TareaId1",
                table: "entregas",
                column: "TareaId1");

            migrationBuilder.AddForeignKey(
                name: "FK_entregas_tareas_TareaId1",
                table: "entregas",
                column: "TareaId1",
                principalTable: "tareas",
                principalColumn: "TareaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_entregas_tareas_TareaId1",
                table: "entregas");

            migrationBuilder.DropIndex(
                name: "IX_entregas_TareaId1",
                table: "entregas");

            migrationBuilder.DropColumn(
                name: "TareaId1",
                table: "entregas");
        }
    }
}
