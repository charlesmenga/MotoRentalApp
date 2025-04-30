using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MotoRentalApp.Migrations
{
    /// <inheritdoc />
    public partial class AddMotos2024Table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ValorTotal",
                table: "Locacoes",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "Motos2024",
                columns: table => new
                {
                    Identificador = table.Column<string>(type: "text", nullable: false),
                    Ano = table.Column<int>(type: "integer", nullable: false),
                    Modelo = table.Column<string>(type: "text", nullable: false),
                    Placa = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Motos2024", x => x.Identificador);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Motos2024_Placa",
                table: "Motos2024",
                column: "Placa",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Motos2024");

            migrationBuilder.DropColumn(
                name: "ValorTotal",
                table: "Locacoes");
        }
    }
}
