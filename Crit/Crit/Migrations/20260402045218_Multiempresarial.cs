using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Crit.Migrations
{
    /// <inheritdoc />
    public partial class Multiempresarial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Productos_Codigo",
                table: "Productos");

            migrationBuilder.AddColumn<int>(
                name: "EmpresaId",
                table: "ServiciosCliente",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EmpresaId",
                table: "Servicios",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EmpresaId",
                table: "Queja",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Proveedores",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EmpresaId",
                table: "Cotizaciones",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EmpresaId",
                table: "Almacenes",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ServiciosCliente_EmpresaId",
                table: "ServiciosCliente",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_Servicios_EmpresaId",
                table: "Servicios",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_Queja_EmpresaId",
                table: "Queja",
                column: "EmpresaId");

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
                name: "IX_Cotizaciones_EmpresaId",
                table: "Cotizaciones",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_Email_EmpresaId",
                table: "Clientes",
                columns: new[] { "Email", "EmpresaId" },
                unique: true,
                filter: "[EmpresaId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Almacenes_EmpresaId",
                table: "Almacenes",
                column: "EmpresaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Almacenes_Empresas_EmpresaId",
                table: "Almacenes",
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Almacenes_Empresas_EmpresaId",
                table: "Almacenes");

            migrationBuilder.DropForeignKey(
                name: "FK_Cotizaciones_Empresas_EmpresaId",
                table: "Cotizaciones");

            migrationBuilder.DropForeignKey(
                name: "FK_Queja_Empresas_EmpresaId",
                table: "Queja");

            migrationBuilder.DropForeignKey(
                name: "FK_Servicios_Empresas_EmpresaId",
                table: "Servicios");

            migrationBuilder.DropForeignKey(
                name: "FK_ServiciosCliente_Empresas_EmpresaId",
                table: "ServiciosCliente");

            migrationBuilder.DropIndex(
                name: "IX_ServiciosCliente_EmpresaId",
                table: "ServiciosCliente");

            migrationBuilder.DropIndex(
                name: "IX_Servicios_EmpresaId",
                table: "Servicios");

            migrationBuilder.DropIndex(
                name: "IX_Queja_EmpresaId",
                table: "Queja");

            migrationBuilder.DropIndex(
                name: "IX_Proveedores_Email_EmpresaId",
                table: "Proveedores");

            migrationBuilder.DropIndex(
                name: "IX_Productos_Codigo_EmpresaId",
                table: "Productos");

            migrationBuilder.DropIndex(
                name: "IX_Cotizaciones_EmpresaId",
                table: "Cotizaciones");

            migrationBuilder.DropIndex(
                name: "IX_Clientes_Email_EmpresaId",
                table: "Clientes");

            migrationBuilder.DropIndex(
                name: "IX_Almacenes_EmpresaId",
                table: "Almacenes");

            migrationBuilder.DropColumn(
                name: "EmpresaId",
                table: "ServiciosCliente");

            migrationBuilder.DropColumn(
                name: "EmpresaId",
                table: "Servicios");

            migrationBuilder.DropColumn(
                name: "EmpresaId",
                table: "Queja");

            migrationBuilder.DropColumn(
                name: "EmpresaId",
                table: "Cotizaciones");

            migrationBuilder.DropColumn(
                name: "EmpresaId",
                table: "Almacenes");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Proveedores",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Productos_Codigo",
                table: "Productos",
                column: "Codigo",
                unique: true);
        }
    }
}
