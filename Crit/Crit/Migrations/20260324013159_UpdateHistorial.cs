using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Crit.Migrations
{
    /// <inheritdoc />
    public partial class UpdateHistorial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "PrecioUnitario",
                table: "DetalleCompra",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaFactura",
                table: "Compra",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "FolioFactura",
                table: "Compra",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RFCProveedor",
                table: "Compra",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SerieFactura",
                table: "Compra",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Kardex",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductoId = table.Column<int>(type: "int", nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TipoMovimiento = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Cantidad = table.Column<int>(type: "int", nullable: false),
                    CostoUnitario = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    StockAnterior = table.Column<int>(type: "int", nullable: false),
                    StockNuevo = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Kardex", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Kardex_Productos_ProductoId",
                        column: x => x.ProductoId,
                        principalTable: "Productos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Kardex_ProductoId",
                table: "Kardex",
                column: "ProductoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Kardex");

            migrationBuilder.DropColumn(
                name: "PrecioUnitario",
                table: "DetalleCompra");

            migrationBuilder.DropColumn(
                name: "FechaFactura",
                table: "Compra");

            migrationBuilder.DropColumn(
                name: "FolioFactura",
                table: "Compra");

            migrationBuilder.DropColumn(
                name: "RFCProveedor",
                table: "Compra");

            migrationBuilder.DropColumn(
                name: "SerieFactura",
                table: "Compra");
        }
    }
}
