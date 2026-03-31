using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Crit.Migrations
{
    /// <inheritdoc />
    public partial class add_reabastecimiento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OrdenesReabastecimiento",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProductoId = table.Column<int>(type: "int", nullable: false),
                    AlmacenId = table.Column<int>(type: "int", nullable: false),
                    StockActual = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    StockMinimo = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CantidadSugerida = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    TipoSugerido = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Observaciones = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrdenesReabastecimiento", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrdenesReabastecimiento_Almacenes_AlmacenId",
                        column: x => x.AlmacenId,
                        principalTable: "Almacenes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_OrdenesReabastecimiento_Productos_ProductoId",
                        column: x => x.ProductoId,
                        principalTable: "Productos",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TraspasosAlmacen",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AlmacenOrigenId = table.Column<int>(type: "int", nullable: false),
                    AlmacenDestinoId = table.Column<int>(type: "int", nullable: false),
                    ProductoId = table.Column<int>(type: "int", nullable: false),
                    Cantidad = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Observaciones = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TraspasosAlmacen", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TraspasosAlmacen_Almacenes_AlmacenDestinoId",
                        column: x => x.AlmacenDestinoId,
                        principalTable: "Almacenes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TraspasosAlmacen_Almacenes_AlmacenOrigenId",
                        column: x => x.AlmacenOrigenId,
                        principalTable: "Almacenes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TraspasosAlmacen_Productos_ProductoId",
                        column: x => x.ProductoId,
                        principalTable: "Productos",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrdenesReabastecimiento_AlmacenId",
                table: "OrdenesReabastecimiento",
                column: "AlmacenId");

            migrationBuilder.CreateIndex(
                name: "IX_OrdenesReabastecimiento_ProductoId",
                table: "OrdenesReabastecimiento",
                column: "ProductoId");

            migrationBuilder.CreateIndex(
                name: "IX_TraspasosAlmacen_AlmacenDestinoId",
                table: "TraspasosAlmacen",
                column: "AlmacenDestinoId");

            migrationBuilder.CreateIndex(
                name: "IX_TraspasosAlmacen_AlmacenOrigenId",
                table: "TraspasosAlmacen",
                column: "AlmacenOrigenId");

            migrationBuilder.CreateIndex(
                name: "IX_TraspasosAlmacen_ProductoId",
                table: "TraspasosAlmacen",
                column: "ProductoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrdenesReabastecimiento");

            migrationBuilder.DropTable(
                name: "TraspasosAlmacen");
        }
    }
}
