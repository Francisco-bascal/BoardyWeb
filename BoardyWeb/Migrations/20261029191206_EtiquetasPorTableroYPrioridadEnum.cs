using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoardyWeb.Migrations
{
    /// <inheritdoc />
    public partial class EtiquetasPorTableroYPrioridadEnum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Las etiquetas existentes eran globales y no pueden asignarse a un tablero;
            // el DbSeeder vuelve a crear las del tablero de demostración.
            migrationBuilder.Sql("DELETE FROM TareaEtiquetas");
            migrationBuilder.Sql("DELETE FROM Etiquetas");

            migrationBuilder.DropIndex(
                name: "IX_Etiquetas_Nombre",
                table: "Etiquetas");

            migrationBuilder.AddColumn<int>(
                name: "TableroId",
                table: "Etiquetas",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Etiquetas_TableroId_Nombre",
                table: "Etiquetas",
                columns: new[] { "TableroId", "Nombre" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Etiquetas_Tableros_TableroId",
                table: "Etiquetas",
                column: "TableroId",
                principalTable: "Tableros",
                principalColumn: "TableroId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Etiquetas_Tableros_TableroId",
                table: "Etiquetas");

            migrationBuilder.DropIndex(
                name: "IX_Etiquetas_TableroId_Nombre",
                table: "Etiquetas");

            migrationBuilder.DropColumn(
                name: "TableroId",
                table: "Etiquetas");

            migrationBuilder.CreateIndex(
                name: "IX_Etiquetas_Nombre",
                table: "Etiquetas",
                column: "Nombre",
                unique: true);
        }
    }
}
