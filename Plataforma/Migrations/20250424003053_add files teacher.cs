using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Plataforma.Migrations
{
    /// <inheritdoc />
    public partial class addfilesteacher : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProfesorArchivoUrl",
                table: "clases",
                type: "character varying(2048)",
                maxLength: 2048,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ProfesorFileName",
                table: "clases",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ProfesorFileType",
                table: "clases",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "ProfesorFileUploadDate",
                table: "clases",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProfesorArchivoUrl",
                table: "clases");

            migrationBuilder.DropColumn(
                name: "ProfesorFileName",
                table: "clases");

            migrationBuilder.DropColumn(
                name: "ProfesorFileType",
                table: "clases");

            migrationBuilder.DropColumn(
                name: "ProfesorFileUploadDate",
                table: "clases");
        }
    }
}
