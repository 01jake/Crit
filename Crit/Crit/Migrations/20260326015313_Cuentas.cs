using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Crit.Migrations
{
    /// <inheritdoc />
    public partial class Cuentas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "EsCredito",
                table: "Ventas",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "EsCredito",
                table: "Compra",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EsCredito",
                table: "Ventas");

            migrationBuilder.DropColumn(
                name: "EsCredito",
                table: "Compra");
        }
    }
}
