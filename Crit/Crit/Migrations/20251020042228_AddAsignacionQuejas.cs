using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Crit.Migrations
{
    /// <inheritdoc />
    public partial class AddAsignacionQuejas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EmpleadoAsignadoId",
                table: "Quejas",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaAsignacion",
                table: "Quejas",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaResolucion",
                table: "Quejas",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmpleadoAsignadoId",
                table: "Queja",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmpleadoAsignadoUserName",
                table: "Queja",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaAsignacion",
                table: "Queja",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaResolucion",
                table: "Queja",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Quejas_EmpleadoAsignadoId",
                table: "Quejas",
                column: "EmpleadoAsignadoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Quejas_AspNetUsers_EmpleadoAsignadoId",
                table: "Quejas",
                column: "EmpleadoAsignadoId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Quejas_AspNetUsers_EmpleadoAsignadoId",
                table: "Quejas");

            migrationBuilder.DropIndex(
                name: "IX_Quejas_EmpleadoAsignadoId",
                table: "Quejas");

            migrationBuilder.DropColumn(
                name: "EmpleadoAsignadoId",
                table: "Quejas");

            migrationBuilder.DropColumn(
                name: "FechaAsignacion",
                table: "Quejas");

            migrationBuilder.DropColumn(
                name: "FechaResolucion",
                table: "Quejas");

            migrationBuilder.DropColumn(
                name: "EmpleadoAsignadoId",
                table: "Queja");

            migrationBuilder.DropColumn(
                name: "EmpleadoAsignadoUserName",
                table: "Queja");

            migrationBuilder.DropColumn(
                name: "FechaAsignacion",
                table: "Queja");

            migrationBuilder.DropColumn(
                name: "FechaResolucion",
                table: "Queja");
        }
    }
}
