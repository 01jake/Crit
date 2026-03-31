using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Crit.Migrations
{
    /// <inheritdoc />
    public partial class add_relaciones_reabastecimiento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CompraId",
                table: "OrdenesReabastecimiento",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TraspasoAlmacenId",
                table: "OrdenesReabastecimiento",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrdenesReabastecimiento_CompraId",
                table: "OrdenesReabastecimiento",
                column: "CompraId");

            migrationBuilder.CreateIndex(
                name: "IX_OrdenesReabastecimiento_TraspasoAlmacenId",
                table: "OrdenesReabastecimiento",
                column: "TraspasoAlmacenId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrdenesReabastecimiento_Compra_CompraId",
                table: "OrdenesReabastecimiento",
                column: "CompraId",
                principalTable: "Compra",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_OrdenesReabastecimiento_TraspasosAlmacen_TraspasoAlmacenId",
                table: "OrdenesReabastecimiento",
                column: "TraspasoAlmacenId",
                principalTable: "TraspasosAlmacen",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrdenesReabastecimiento_Compra_CompraId",
                table: "OrdenesReabastecimiento");

            migrationBuilder.DropForeignKey(
                name: "FK_OrdenesReabastecimiento_TraspasosAlmacen_TraspasoAlmacenId",
                table: "OrdenesReabastecimiento");

            migrationBuilder.DropIndex(
                name: "IX_OrdenesReabastecimiento_CompraId",
                table: "OrdenesReabastecimiento");

            migrationBuilder.DropIndex(
                name: "IX_OrdenesReabastecimiento_TraspasoAlmacenId",
                table: "OrdenesReabastecimiento");

            migrationBuilder.DropColumn(
                name: "CompraId",
                table: "OrdenesReabastecimiento");

            migrationBuilder.DropColumn(
                name: "TraspasoAlmacenId",
                table: "OrdenesReabastecimiento");
        }
    }
}
