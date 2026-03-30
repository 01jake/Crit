using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Crit.Migrations
{
    /// <inheritdoc />
    public partial class almacenes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AlmacenId",
                table: "Ventas",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "AlmacenId",
                table: "Compra",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Almacenes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Clave = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Direccion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Almacenes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UbicacionAlmacen",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AlmacenId = table.Column<int>(type: "int", nullable: false),
                    Codigo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Activa = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UbicacionAlmacen", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UbicacionAlmacen_Almacenes_AlmacenId",
                        column: x => x.AlmacenId,
                        principalTable: "Almacenes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InventarioPorAlmacen",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductoId = table.Column<int>(type: "int", nullable: false),
                    AlmacenId = table.Column<int>(type: "int", nullable: false),
                    UbicacionAlmacenId = table.Column<int>(type: "int", nullable: true),
                    Stock = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    StockMinimo = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    StockMaximo = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventarioPorAlmacen", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InventarioPorAlmacen_Almacenes_AlmacenId",
                        column: x => x.AlmacenId,
                        principalTable: "Almacenes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_InventarioPorAlmacen_Productos_ProductoId",
                        column: x => x.ProductoId,
                        principalTable: "Productos",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_InventarioPorAlmacen_UbicacionAlmacen_UbicacionAlmacenId",
                        column: x => x.UbicacionAlmacenId,
                        principalTable: "UbicacionAlmacen",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "MovimientosInventario",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProductoId = table.Column<int>(type: "int", nullable: false),
                    AlmacenId = table.Column<int>(type: "int", nullable: false),
                    UbicacionAlmacenId = table.Column<int>(type: "int", nullable: true),
                    TipoMovimiento = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Cantidad = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    StockAnterior = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    StockNuevo = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Referencia = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Observaciones = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CompraId = table.Column<int>(type: "int", nullable: true),
                    VentaId = table.Column<int>(type: "int", nullable: true),
                    TraspasoId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovimientosInventario", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MovimientosInventario_Almacenes_AlmacenId",
                        column: x => x.AlmacenId,
                        principalTable: "Almacenes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MovimientosInventario_Productos_ProductoId",
                        column: x => x.ProductoId,
                        principalTable: "Productos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MovimientosInventario_UbicacionAlmacen_UbicacionAlmacenId",
                        column: x => x.UbicacionAlmacenId,
                        principalTable: "UbicacionAlmacen",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Ventas_AlmacenId",
                table: "Ventas",
                column: "AlmacenId");

            migrationBuilder.CreateIndex(
                name: "IX_Compra_AlmacenId",
                table: "Compra",
                column: "AlmacenId");

            migrationBuilder.CreateIndex(
                name: "IX_InventarioPorAlmacen_AlmacenId",
                table: "InventarioPorAlmacen",
                column: "AlmacenId");

            migrationBuilder.CreateIndex(
                name: "IX_InventarioPorAlmacen_ProductoId_AlmacenId",
                table: "InventarioPorAlmacen",
                columns: new[] { "ProductoId", "AlmacenId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InventarioPorAlmacen_UbicacionAlmacenId",
                table: "InventarioPorAlmacen",
                column: "UbicacionAlmacenId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosInventario_AlmacenId",
                table: "MovimientosInventario",
                column: "AlmacenId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosInventario_ProductoId",
                table: "MovimientosInventario",
                column: "ProductoId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosInventario_UbicacionAlmacenId",
                table: "MovimientosInventario",
                column: "UbicacionAlmacenId");

            migrationBuilder.CreateIndex(
                name: "IX_UbicacionAlmacen_AlmacenId",
                table: "UbicacionAlmacen",
                column: "AlmacenId");

            migrationBuilder.AddForeignKey(
                name: "FK_Compra_Almacenes_AlmacenId",
                table: "Compra",
                column: "AlmacenId",
                principalTable: "Almacenes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Ventas_Almacenes_AlmacenId",
                table: "Ventas",
                column: "AlmacenId",
                principalTable: "Almacenes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Compra_Almacenes_AlmacenId",
                table: "Compra");

            migrationBuilder.DropForeignKey(
                name: "FK_Ventas_Almacenes_AlmacenId",
                table: "Ventas");

            migrationBuilder.DropTable(
                name: "InventarioPorAlmacen");

            migrationBuilder.DropTable(
                name: "MovimientosInventario");

            migrationBuilder.DropTable(
                name: "UbicacionAlmacen");

            migrationBuilder.DropTable(
                name: "Almacenes");

            migrationBuilder.DropIndex(
                name: "IX_Ventas_AlmacenId",
                table: "Ventas");

            migrationBuilder.DropIndex(
                name: "IX_Compra_AlmacenId",
                table: "Compra");

            migrationBuilder.DropColumn(
                name: "AlmacenId",
                table: "Ventas");

            migrationBuilder.DropColumn(
                name: "AlmacenId",
                table: "Compra");
        }
    }
}
