using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Crit.Migrations
{
    /// <inheritdoc />
    public partial class empresa_nullable_en_users : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EmpresaId",
                table: "Ventas",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EmpresaId",
                table: "TraspasosAlmacen",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EmpresaId",
                table: "Proveedores",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EmpresaId",
                table: "Productos",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EmpresaId",
                table: "PagosProveedor",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EmpresaId",
                table: "PagosCliente",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EmpresaId",
                table: "OrdenesReabastecimiento",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EmpresaId",
                table: "MovimientosInventario",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EmpresaId",
                table: "InventarioPorAlmacen",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EmpresaId",
                table: "Gastos",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EmpresaId",
                table: "CuentasPorPagar",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EmpresaId",
                table: "CuentasPorCobrar",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EmpresaId",
                table: "Compra",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EmpresaId",
                table: "CajaSesiones",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EmpresaId",
                table: "CajaMovimientos",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EmpresaId",
                table: "AspNetUsers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NombreCompleto",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Empresas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    RFC = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Dominio = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Activa = table.Column<bool>(type: "bit", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Empresas", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Ventas_EmpresaId",
                table: "Ventas",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_TraspasosAlmacen_EmpresaId",
                table: "TraspasosAlmacen",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_Proveedores_EmpresaId",
                table: "Proveedores",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_Productos_EmpresaId",
                table: "Productos",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_PagosProveedor_EmpresaId",
                table: "PagosProveedor",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_PagosCliente_EmpresaId",
                table: "PagosCliente",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_OrdenesReabastecimiento_EmpresaId",
                table: "OrdenesReabastecimiento",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosInventario_EmpresaId",
                table: "MovimientosInventario",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_InventarioPorAlmacen_EmpresaId",
                table: "InventarioPorAlmacen",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_Gastos_EmpresaId",
                table: "Gastos",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_CuentasPorPagar_EmpresaId",
                table: "CuentasPorPagar",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_CuentasPorCobrar_EmpresaId",
                table: "CuentasPorCobrar",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_Compra_EmpresaId",
                table: "Compra",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_CajaSesiones_EmpresaId",
                table: "CajaSesiones",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_CajaMovimientos_EmpresaId",
                table: "CajaMovimientos",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_EmpresaId",
                table: "AspNetUsers",
                column: "EmpresaId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Empresas_EmpresaId",
                table: "AspNetUsers",
                column: "EmpresaId",
                principalTable: "Empresas",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CajaMovimientos_Empresas_EmpresaId",
                table: "CajaMovimientos",
                column: "EmpresaId",
                principalTable: "Empresas",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CajaSesiones_Empresas_EmpresaId",
                table: "CajaSesiones",
                column: "EmpresaId",
                principalTable: "Empresas",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Compra_Empresas_EmpresaId",
                table: "Compra",
                column: "EmpresaId",
                principalTable: "Empresas",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CuentasPorCobrar_Empresas_EmpresaId",
                table: "CuentasPorCobrar",
                column: "EmpresaId",
                principalTable: "Empresas",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CuentasPorPagar_Empresas_EmpresaId",
                table: "CuentasPorPagar",
                column: "EmpresaId",
                principalTable: "Empresas",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Gastos_Empresas_EmpresaId",
                table: "Gastos",
                column: "EmpresaId",
                principalTable: "Empresas",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_InventarioPorAlmacen_Empresas_EmpresaId",
                table: "InventarioPorAlmacen",
                column: "EmpresaId",
                principalTable: "Empresas",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MovimientosInventario_Empresas_EmpresaId",
                table: "MovimientosInventario",
                column: "EmpresaId",
                principalTable: "Empresas",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_OrdenesReabastecimiento_Empresas_EmpresaId",
                table: "OrdenesReabastecimiento",
                column: "EmpresaId",
                principalTable: "Empresas",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PagosCliente_Empresas_EmpresaId",
                table: "PagosCliente",
                column: "EmpresaId",
                principalTable: "Empresas",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PagosProveedor_Empresas_EmpresaId",
                table: "PagosProveedor",
                column: "EmpresaId",
                principalTable: "Empresas",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Productos_Empresas_EmpresaId",
                table: "Productos",
                column: "EmpresaId",
                principalTable: "Empresas",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Proveedores_Empresas_EmpresaId",
                table: "Proveedores",
                column: "EmpresaId",
                principalTable: "Empresas",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TraspasosAlmacen_Empresas_EmpresaId",
                table: "TraspasosAlmacen",
                column: "EmpresaId",
                principalTable: "Empresas",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Ventas_Empresas_EmpresaId",
                table: "Ventas",
                column: "EmpresaId",
                principalTable: "Empresas",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Empresas_EmpresaId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_CajaMovimientos_Empresas_EmpresaId",
                table: "CajaMovimientos");

            migrationBuilder.DropForeignKey(
                name: "FK_CajaSesiones_Empresas_EmpresaId",
                table: "CajaSesiones");

            migrationBuilder.DropForeignKey(
                name: "FK_Compra_Empresas_EmpresaId",
                table: "Compra");

            migrationBuilder.DropForeignKey(
                name: "FK_CuentasPorCobrar_Empresas_EmpresaId",
                table: "CuentasPorCobrar");

            migrationBuilder.DropForeignKey(
                name: "FK_CuentasPorPagar_Empresas_EmpresaId",
                table: "CuentasPorPagar");

            migrationBuilder.DropForeignKey(
                name: "FK_Gastos_Empresas_EmpresaId",
                table: "Gastos");

            migrationBuilder.DropForeignKey(
                name: "FK_InventarioPorAlmacen_Empresas_EmpresaId",
                table: "InventarioPorAlmacen");

            migrationBuilder.DropForeignKey(
                name: "FK_MovimientosInventario_Empresas_EmpresaId",
                table: "MovimientosInventario");

            migrationBuilder.DropForeignKey(
                name: "FK_OrdenesReabastecimiento_Empresas_EmpresaId",
                table: "OrdenesReabastecimiento");

            migrationBuilder.DropForeignKey(
                name: "FK_PagosCliente_Empresas_EmpresaId",
                table: "PagosCliente");

            migrationBuilder.DropForeignKey(
                name: "FK_PagosProveedor_Empresas_EmpresaId",
                table: "PagosProveedor");

            migrationBuilder.DropForeignKey(
                name: "FK_Productos_Empresas_EmpresaId",
                table: "Productos");

            migrationBuilder.DropForeignKey(
                name: "FK_Proveedores_Empresas_EmpresaId",
                table: "Proveedores");

            migrationBuilder.DropForeignKey(
                name: "FK_TraspasosAlmacen_Empresas_EmpresaId",
                table: "TraspasosAlmacen");

            migrationBuilder.DropForeignKey(
                name: "FK_Ventas_Empresas_EmpresaId",
                table: "Ventas");

            migrationBuilder.DropTable(
                name: "Empresas");

            migrationBuilder.DropIndex(
                name: "IX_Ventas_EmpresaId",
                table: "Ventas");

            migrationBuilder.DropIndex(
                name: "IX_TraspasosAlmacen_EmpresaId",
                table: "TraspasosAlmacen");

            migrationBuilder.DropIndex(
                name: "IX_Proveedores_EmpresaId",
                table: "Proveedores");

            migrationBuilder.DropIndex(
                name: "IX_Productos_EmpresaId",
                table: "Productos");

            migrationBuilder.DropIndex(
                name: "IX_PagosProveedor_EmpresaId",
                table: "PagosProveedor");

            migrationBuilder.DropIndex(
                name: "IX_PagosCliente_EmpresaId",
                table: "PagosCliente");

            migrationBuilder.DropIndex(
                name: "IX_OrdenesReabastecimiento_EmpresaId",
                table: "OrdenesReabastecimiento");

            migrationBuilder.DropIndex(
                name: "IX_MovimientosInventario_EmpresaId",
                table: "MovimientosInventario");

            migrationBuilder.DropIndex(
                name: "IX_InventarioPorAlmacen_EmpresaId",
                table: "InventarioPorAlmacen");

            migrationBuilder.DropIndex(
                name: "IX_Gastos_EmpresaId",
                table: "Gastos");

            migrationBuilder.DropIndex(
                name: "IX_CuentasPorPagar_EmpresaId",
                table: "CuentasPorPagar");

            migrationBuilder.DropIndex(
                name: "IX_CuentasPorCobrar_EmpresaId",
                table: "CuentasPorCobrar");

            migrationBuilder.DropIndex(
                name: "IX_Compra_EmpresaId",
                table: "Compra");

            migrationBuilder.DropIndex(
                name: "IX_CajaSesiones_EmpresaId",
                table: "CajaSesiones");

            migrationBuilder.DropIndex(
                name: "IX_CajaMovimientos_EmpresaId",
                table: "CajaMovimientos");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_EmpresaId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "EmpresaId",
                table: "Ventas");

            migrationBuilder.DropColumn(
                name: "EmpresaId",
                table: "TraspasosAlmacen");

            migrationBuilder.DropColumn(
                name: "EmpresaId",
                table: "Proveedores");

            migrationBuilder.DropColumn(
                name: "EmpresaId",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "EmpresaId",
                table: "PagosProveedor");

            migrationBuilder.DropColumn(
                name: "EmpresaId",
                table: "PagosCliente");

            migrationBuilder.DropColumn(
                name: "EmpresaId",
                table: "OrdenesReabastecimiento");

            migrationBuilder.DropColumn(
                name: "EmpresaId",
                table: "MovimientosInventario");

            migrationBuilder.DropColumn(
                name: "EmpresaId",
                table: "InventarioPorAlmacen");

            migrationBuilder.DropColumn(
                name: "EmpresaId",
                table: "Gastos");

            migrationBuilder.DropColumn(
                name: "EmpresaId",
                table: "CuentasPorPagar");

            migrationBuilder.DropColumn(
                name: "EmpresaId",
                table: "CuentasPorCobrar");

            migrationBuilder.DropColumn(
                name: "EmpresaId",
                table: "Compra");

            migrationBuilder.DropColumn(
                name: "EmpresaId",
                table: "CajaSesiones");

            migrationBuilder.DropColumn(
                name: "EmpresaId",
                table: "CajaMovimientos");

            migrationBuilder.DropColumn(
                name: "EmpresaId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "NombreCompleto",
                table: "AspNetUsers");
        }
    }
}
