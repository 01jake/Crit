using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Crit.Migrations
{
    /// <inheritdoc />
    public partial class AgregarProductos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cotizacion_Clientes_ClienteId",
                table: "Cotizacion");

            migrationBuilder.DropForeignKey(
                name: "FK_DetalleCotizacion_Cotizacion_CotizacionId",
                table: "DetalleCotizacion");

            migrationBuilder.DropForeignKey(
                name: "FK_DetalleCotizacion_Producto_ProductoId",
                table: "DetalleCotizacion");

            migrationBuilder.DropForeignKey(
                name: "FK_DetalleVenta_Producto_ProductoId",
                table: "DetalleVenta");

            migrationBuilder.DropForeignKey(
                name: "FK_DetalleVenta_Venta_VentaId",
                table: "DetalleVenta");

            migrationBuilder.DropForeignKey(
                name: "FK_ServiciosCliente_Servicio_ServicioId",
                table: "ServiciosCliente");

            migrationBuilder.DropForeignKey(
                name: "FK_Venta_Clientes_ClienteId",
                table: "Venta");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Venta",
                table: "Venta");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Servicio",
                table: "Servicio");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Producto",
                table: "Producto");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DetalleVenta",
                table: "DetalleVenta");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DetalleCotizacion",
                table: "DetalleCotizacion");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Cotizacion",
                table: "Cotizacion");

            migrationBuilder.RenameTable(
                name: "Venta",
                newName: "Ventas");

            migrationBuilder.RenameTable(
                name: "Servicio",
                newName: "Servicios");

            migrationBuilder.RenameTable(
                name: "Producto",
                newName: "Productos");

            migrationBuilder.RenameTable(
                name: "DetalleVenta",
                newName: "DetallesVenta");

            migrationBuilder.RenameTable(
                name: "DetalleCotizacion",
                newName: "DetallesCotizacion");

            migrationBuilder.RenameTable(
                name: "Cotizacion",
                newName: "Cotizaciones");

            migrationBuilder.RenameIndex(
                name: "IX_Venta_ClienteId",
                table: "Ventas",
                newName: "IX_Ventas_ClienteId");

            migrationBuilder.RenameIndex(
                name: "IX_DetalleVenta_VentaId",
                table: "DetallesVenta",
                newName: "IX_DetallesVenta_VentaId");

            migrationBuilder.RenameIndex(
                name: "IX_DetalleVenta_ProductoId",
                table: "DetallesVenta",
                newName: "IX_DetallesVenta_ProductoId");

            migrationBuilder.RenameIndex(
                name: "IX_DetalleCotizacion_ProductoId",
                table: "DetallesCotizacion",
                newName: "IX_DetallesCotizacion_ProductoId");

            migrationBuilder.RenameIndex(
                name: "IX_DetalleCotizacion_CotizacionId",
                table: "DetallesCotizacion",
                newName: "IX_DetallesCotizacion_CotizacionId");

            migrationBuilder.RenameIndex(
                name: "IX_Cotizacion_ClienteId",
                table: "Cotizaciones",
                newName: "IX_Cotizaciones_ClienteId");

            migrationBuilder.AlterColumn<string>(
                name: "NumeroVenta",
                table: "Ventas",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "NumeroCotizacion",
                table: "Cotizaciones",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Ventas",
                table: "Ventas",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Servicios",
                table: "Servicios",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Productos",
                table: "Productos",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DetallesVenta",
                table: "DetallesVenta",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DetallesCotizacion",
                table: "DetallesCotizacion",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Cotizaciones",
                table: "Cotizaciones",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Ventas_NumeroVenta",
                table: "Ventas",
                column: "NumeroVenta",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Productos_Codigo",
                table: "Productos",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cotizaciones_NumeroCotizacion",
                table: "Cotizaciones",
                column: "NumeroCotizacion",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Cotizaciones_Clientes_ClienteId",
                table: "Cotizaciones",
                column: "ClienteId",
                principalTable: "Clientes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DetallesCotizacion_Cotizaciones_CotizacionId",
                table: "DetallesCotizacion",
                column: "CotizacionId",
                principalTable: "Cotizaciones",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DetallesCotizacion_Productos_ProductoId",
                table: "DetallesCotizacion",
                column: "ProductoId",
                principalTable: "Productos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DetallesVenta_Productos_ProductoId",
                table: "DetallesVenta",
                column: "ProductoId",
                principalTable: "Productos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DetallesVenta_Ventas_VentaId",
                table: "DetallesVenta",
                column: "VentaId",
                principalTable: "Ventas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ServiciosCliente_Servicios_ServicioId",
                table: "ServiciosCliente",
                column: "ServicioId",
                principalTable: "Servicios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Ventas_Clientes_ClienteId",
                table: "Ventas",
                column: "ClienteId",
                principalTable: "Clientes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cotizaciones_Clientes_ClienteId",
                table: "Cotizaciones");

            migrationBuilder.DropForeignKey(
                name: "FK_DetallesCotizacion_Cotizaciones_CotizacionId",
                table: "DetallesCotizacion");

            migrationBuilder.DropForeignKey(
                name: "FK_DetallesCotizacion_Productos_ProductoId",
                table: "DetallesCotizacion");

            migrationBuilder.DropForeignKey(
                name: "FK_DetallesVenta_Productos_ProductoId",
                table: "DetallesVenta");

            migrationBuilder.DropForeignKey(
                name: "FK_DetallesVenta_Ventas_VentaId",
                table: "DetallesVenta");

            migrationBuilder.DropForeignKey(
                name: "FK_ServiciosCliente_Servicios_ServicioId",
                table: "ServiciosCliente");

            migrationBuilder.DropForeignKey(
                name: "FK_Ventas_Clientes_ClienteId",
                table: "Ventas");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Ventas",
                table: "Ventas");

            migrationBuilder.DropIndex(
                name: "IX_Ventas_NumeroVenta",
                table: "Ventas");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Servicios",
                table: "Servicios");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Productos",
                table: "Productos");

            migrationBuilder.DropIndex(
                name: "IX_Productos_Codigo",
                table: "Productos");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DetallesVenta",
                table: "DetallesVenta");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DetallesCotizacion",
                table: "DetallesCotizacion");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Cotizaciones",
                table: "Cotizaciones");

            migrationBuilder.DropIndex(
                name: "IX_Cotizaciones_NumeroCotizacion",
                table: "Cotizaciones");

            migrationBuilder.RenameTable(
                name: "Ventas",
                newName: "Venta");

            migrationBuilder.RenameTable(
                name: "Servicios",
                newName: "Servicio");

            migrationBuilder.RenameTable(
                name: "Productos",
                newName: "Producto");

            migrationBuilder.RenameTable(
                name: "DetallesVenta",
                newName: "DetalleVenta");

            migrationBuilder.RenameTable(
                name: "DetallesCotizacion",
                newName: "DetalleCotizacion");

            migrationBuilder.RenameTable(
                name: "Cotizaciones",
                newName: "Cotizacion");

            migrationBuilder.RenameIndex(
                name: "IX_Ventas_ClienteId",
                table: "Venta",
                newName: "IX_Venta_ClienteId");

            migrationBuilder.RenameIndex(
                name: "IX_DetallesVenta_VentaId",
                table: "DetalleVenta",
                newName: "IX_DetalleVenta_VentaId");

            migrationBuilder.RenameIndex(
                name: "IX_DetallesVenta_ProductoId",
                table: "DetalleVenta",
                newName: "IX_DetalleVenta_ProductoId");

            migrationBuilder.RenameIndex(
                name: "IX_DetallesCotizacion_ProductoId",
                table: "DetalleCotizacion",
                newName: "IX_DetalleCotizacion_ProductoId");

            migrationBuilder.RenameIndex(
                name: "IX_DetallesCotizacion_CotizacionId",
                table: "DetalleCotizacion",
                newName: "IX_DetalleCotizacion_CotizacionId");

            migrationBuilder.RenameIndex(
                name: "IX_Cotizaciones_ClienteId",
                table: "Cotizacion",
                newName: "IX_Cotizacion_ClienteId");

            migrationBuilder.AlterColumn<string>(
                name: "NumeroVenta",
                table: "Venta",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "NumeroCotizacion",
                table: "Cotizacion",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Venta",
                table: "Venta",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Servicio",
                table: "Servicio",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Producto",
                table: "Producto",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DetalleVenta",
                table: "DetalleVenta",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DetalleCotizacion",
                table: "DetalleCotizacion",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Cotizacion",
                table: "Cotizacion",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Cotizacion_Clientes_ClienteId",
                table: "Cotizacion",
                column: "ClienteId",
                principalTable: "Clientes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DetalleCotizacion_Cotizacion_CotizacionId",
                table: "DetalleCotizacion",
                column: "CotizacionId",
                principalTable: "Cotizacion",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DetalleCotizacion_Producto_ProductoId",
                table: "DetalleCotizacion",
                column: "ProductoId",
                principalTable: "Producto",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DetalleVenta_Producto_ProductoId",
                table: "DetalleVenta",
                column: "ProductoId",
                principalTable: "Producto",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DetalleVenta_Venta_VentaId",
                table: "DetalleVenta",
                column: "VentaId",
                principalTable: "Venta",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ServiciosCliente_Servicio_ServicioId",
                table: "ServiciosCliente",
                column: "ServicioId",
                principalTable: "Servicio",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Venta_Clientes_ClienteId",
                table: "Venta",
                column: "ClienteId",
                principalTable: "Clientes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
