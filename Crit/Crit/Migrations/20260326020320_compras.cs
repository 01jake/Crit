using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Crit.Migrations
{
    /// <inheritdoc />
    public partial class compras : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DiasCredito",
                table: "Ventas",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DiasCredito",
                table: "Compra",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DiasCredito",
                table: "Ventas");

            migrationBuilder.DropColumn(
                name: "DiasCredito",
                table: "Compra");
        }
    }
}
