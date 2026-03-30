using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Crit.Migrations
{
    /// <inheritdoc />
    public partial class AlmacenNullableEnCompraVenta : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Compra_Almacenes_AlmacenId",
                table: "Compra");

            migrationBuilder.DropForeignKey(
                name: "FK_Ventas_Almacenes_AlmacenId",
                table: "Ventas");

            migrationBuilder.AlterColumn<int>(
                name: "AlmacenId",
                table: "Ventas",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "AlmacenId",
                table: "Compra",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Compra_Almacenes_AlmacenId",
                table: "Compra",
                column: "AlmacenId",
                principalTable: "Almacenes",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Ventas_Almacenes_AlmacenId",
                table: "Ventas",
                column: "AlmacenId",
                principalTable: "Almacenes",
                principalColumn: "Id");
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

            migrationBuilder.AlterColumn<int>(
                name: "AlmacenId",
                table: "Ventas",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "AlmacenId",
                table: "Compra",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

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
    }
}
