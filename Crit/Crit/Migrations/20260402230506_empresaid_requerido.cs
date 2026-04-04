using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Crit.Migrations
{
    /// <inheritdoc />
    public partial class empresaid_requerido : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Almacenes_Empresas_EmpresaId",
                table: "Almacenes");

            migrationBuilder.DropForeignKey(
                name: "FK_CajaMovimientos_Empresas_EmpresaId",
                table: "CajaMovimientos");

            migrationBuilder.DropForeignKey(
                name: "FK_CajaSesiones_Empresas_EmpresaId",
                table: "CajaSesiones");

            migrationBuilder.DropForeignKey(
                name: "FK_Clientes_Empresas_EmpresaId",
                table: "Clientes");

            migrationBuilder.DropForeignKey(
                name: "FK_Compra_Empresas_EmpresaId",
                table: "Compra");

            migrationBuilder.DropForeignKey(
                name: "FK_Cotizaciones_Empresas_EmpresaId",
                table: "Cotizaciones");

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
                name: "FK_Queja_Empresas_EmpresaId",
                table: "Queja");

            migrationBuilder.DropForeignKey(
                name: "FK_Servicios_Empresas_EmpresaId",
                table: "Servicios");

            migrationBuilder.DropForeignKey(
                name: "FK_ServiciosCliente_Empresas_EmpresaId",
                table: "ServiciosCliente");

            migrationBuilder.DropForeignKey(
                name: "FK_TraspasosAlmacen_Empresas_EmpresaId",
                table: "TraspasosAlmacen");

            migrationBuilder.DropForeignKey(
                name: "FK_Ventas_Empresas_EmpresaId",
                table: "Ventas");

            migrationBuilder.DropIndex(
                name: "IX_Proveedores_Email_EmpresaId",
                table: "Proveedores");

            migrationBuilder.DropIndex(
                name: "IX_Productos_Codigo_EmpresaId",
                table: "Productos");

            migrationBuilder.DropIndex(
                name: "IX_Clientes_Email_EmpresaId",
                table: "Clientes");

            migrationBuilder.AlterColumn<int>(
                name: "EmpresaId",
                table: "Ventas",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "EmpresaId",
                table: "TraspasosAlmacen",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "EmpresaId",
                table: "ServiciosCliente",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "EmpresaId",
                table: "Servicios",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "EmpresaId",
                table: "Queja",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "EmpresaId",
                table: "Proveedores",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "EmpresaId",
                table: "Productos",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "EmpresaId",
                table: "PagosProveedor",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "EmpresaId",
                table: "PagosCliente",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "EmpresaId",
                table: "OrdenesReabastecimiento",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "EmpresaId",
                table: "MovimientosInventario",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "EmpresaId",
                table: "InventarioPorAlmacen",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "EmpresaId",
                table: "Gastos",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "EmpresaId",
                table: "CuentasPorPagar",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "EmpresaId",
                table: "CuentasPorCobrar",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "EmpresaId",
                table: "Cotizaciones",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "EmpresaId",
                table: "Compra",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "EmpresaId",
                table: "Clientes",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "EmpresaId",
                table: "CajaSesiones",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "EmpresaId",
                table: "CajaMovimientos",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "EmpresaId",
                table: "Almacenes",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Proveedores_Email_EmpresaId",
                table: "Proveedores",
                columns: new[] { "Email", "EmpresaId" },
                unique: true,
                filter: "[Email] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Productos_Codigo_EmpresaId",
                table: "Productos",
                columns: new[] { "Codigo", "EmpresaId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_Email_EmpresaId",
                table: "Clientes",
                columns: new[] { "Email", "EmpresaId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Almacenes_Empresas_EmpresaId",
                table: "Almacenes",
                column: "EmpresaId",
                principalTable: "Empresas",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_CajaMovimientos_Empresas_EmpresaId",
                table: "CajaMovimientos",
                column: "EmpresaId",
                principalTable: "Empresas",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_CajaSesiones_Empresas_EmpresaId",
                table: "CajaSesiones",
                column: "EmpresaId",
                principalTable: "Empresas",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_Clientes_Empresas_EmpresaId",
                table: "Clientes",
                column: "EmpresaId",
                principalTable: "Empresas",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_Compra_Empresas_EmpresaId",
                table: "Compra",
                column: "EmpresaId",
                principalTable: "Empresas",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_Cotizaciones_Empresas_EmpresaId",
                table: "Cotizaciones",
                column: "EmpresaId",
                principalTable: "Empresas",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_CuentasPorCobrar_Empresas_EmpresaId",
                table: "CuentasPorCobrar",
                column: "EmpresaId",
                principalTable: "Empresas",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_CuentasPorPagar_Empresas_EmpresaId",
                table: "CuentasPorPagar",
                column: "EmpresaId",
                principalTable: "Empresas",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_Gastos_Empresas_EmpresaId",
                table: "Gastos",
                column: "EmpresaId",
                principalTable: "Empresas",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_InventarioPorAlmacen_Empresas_EmpresaId",
                table: "InventarioPorAlmacen",
                column: "EmpresaId",
                principalTable: "Empresas",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_MovimientosInventario_Empresas_EmpresaId",
                table: "MovimientosInventario",
                column: "EmpresaId",
                principalTable: "Empresas",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_OrdenesReabastecimiento_Empresas_EmpresaId",
                table: "OrdenesReabastecimiento",
                column: "EmpresaId",
                principalTable: "Empresas",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_PagosCliente_Empresas_EmpresaId",
                table: "PagosCliente",
                column: "EmpresaId",
                principalTable: "Empresas",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_PagosProveedor_Empresas_EmpresaId",
                table: "PagosProveedor",
                column: "EmpresaId",
                principalTable: "Empresas",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_Productos_Empresas_EmpresaId",
                table: "Productos",
                column: "EmpresaId",
                principalTable: "Empresas",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_Proveedores_Empresas_EmpresaId",
                table: "Proveedores",
                column: "EmpresaId",
                principalTable: "Empresas",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_Queja_Empresas_EmpresaId",
                table: "Queja",
                column: "EmpresaId",
                principalTable: "Empresas",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_Servicios_Empresas_EmpresaId",
                table: "Servicios",
                column: "EmpresaId",
                principalTable: "Empresas",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_ServiciosCliente_Empresas_EmpresaId",
                table: "ServiciosCliente",
                column: "EmpresaId",
                principalTable: "Empresas",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_TraspasosAlmacen_Empresas_EmpresaId",
                table: "TraspasosAlmacen",
                column: "EmpresaId",
                principalTable: "Empresas",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_Ventas_Empresas_EmpresaId",
                table: "Ventas",
                column: "EmpresaId",
                principalTable: "Empresas",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Almacenes_Empresas_EmpresaId",
                table: "Almacenes");

            migrationBuilder.DropForeignKey(
                name: "FK_CajaMovimientos_Empresas_EmpresaId",
                table: "CajaMovimientos");

            migrationBuilder.DropForeignKey(
                name: "FK_CajaSesiones_Empresas_EmpresaId",
                table: "CajaSesiones");

            migrationBuilder.DropForeignKey(
                name: "FK_Clientes_Empresas_EmpresaId",
                table: "Clientes");

            migrationBuilder.DropForeignKey(
                name: "FK_Compra_Empresas_EmpresaId",
                table: "Compra");

            migrationBuilder.DropForeignKey(
                name: "FK_Cotizaciones_Empresas_EmpresaId",
                table: "Cotizaciones");

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
                name: "FK_Queja_Empresas_EmpresaId",
                table: "Queja");

            migrationBuilder.DropForeignKey(
                name: "FK_Servicios_Empresas_EmpresaId",
                table: "Servicios");

            migrationBuilder.DropForeignKey(
                name: "FK_ServiciosCliente_Empresas_EmpresaId",
                table: "ServiciosCliente");

            migrationBuilder.DropForeignKey(
                name: "FK_TraspasosAlmacen_Empresas_EmpresaId",
                table: "TraspasosAlmacen");

            migrationBuilder.DropForeignKey(
                name: "FK_Ventas_Empresas_EmpresaId",
                table: "Ventas");

            migrationBuilder.DropIndex(
                name: "IX_Proveedores_Email_EmpresaId",
                table: "Proveedores");

            migrationBuilder.DropIndex(
                name: "IX_Productos_Codigo_EmpresaId",
                table: "Productos");

            migrationBuilder.DropIndex(
                name: "IX_Clientes_Email_EmpresaId",
                table: "Clientes");

            migrationBuilder.AlterColumn<int>(
                name: "EmpresaId",
                table: "Ventas",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "EmpresaId",
                table: "TraspasosAlmacen",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "EmpresaId",
                table: "ServiciosCliente",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "EmpresaId",
                table: "Servicios",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "EmpresaId",
                table: "Queja",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "EmpresaId",
                table: "Proveedores",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "EmpresaId",
                table: "Productos",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "EmpresaId",
                table: "PagosProveedor",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "EmpresaId",
                table: "PagosCliente",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "EmpresaId",
                table: "OrdenesReabastecimiento",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "EmpresaId",
                table: "MovimientosInventario",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "EmpresaId",
                table: "InventarioPorAlmacen",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "EmpresaId",
                table: "Gastos",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "EmpresaId",
                table: "CuentasPorPagar",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "EmpresaId",
                table: "CuentasPorCobrar",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "EmpresaId",
                table: "Cotizaciones",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "EmpresaId",
                table: "Compra",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "EmpresaId",
                table: "Clientes",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "EmpresaId",
                table: "CajaSesiones",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "EmpresaId",
                table: "CajaMovimientos",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "EmpresaId",
                table: "Almacenes",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateIndex(
                name: "IX_Proveedores_Email_EmpresaId",
                table: "Proveedores",
                columns: new[] { "Email", "EmpresaId" },
                unique: true,
                filter: "[Email] IS NOT NULL AND [EmpresaId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Productos_Codigo_EmpresaId",
                table: "Productos",
                columns: new[] { "Codigo", "EmpresaId" },
                unique: true,
                filter: "[EmpresaId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_Email_EmpresaId",
                table: "Clientes",
                columns: new[] { "Email", "EmpresaId" },
                unique: true,
                filter: "[EmpresaId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Almacenes_Empresas_EmpresaId",
                table: "Almacenes",
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
                name: "FK_Clientes_Empresas_EmpresaId",
                table: "Clientes",
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
                name: "FK_Cotizaciones_Empresas_EmpresaId",
                table: "Cotizaciones",
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
                name: "FK_Queja_Empresas_EmpresaId",
                table: "Queja",
                column: "EmpresaId",
                principalTable: "Empresas",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Servicios_Empresas_EmpresaId",
                table: "Servicios",
                column: "EmpresaId",
                principalTable: "Empresas",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ServiciosCliente_Empresas_EmpresaId",
                table: "ServiciosCliente",
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
    }
}
