using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Crit.Migrations
{
    /// <inheritdoc />
    public partial class Empresa_Fix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
    name: "Empresas");
        }
    }
}
