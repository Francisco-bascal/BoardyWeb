using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoardyWeb.Migrations
{
    /// <inheritdoc />
    public partial class EstadosPorTablero : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EstadoTareas_Nombre",
                table: "EstadoTareas");

            migrationBuilder.AddColumn<int>(
                name: "TableroId",
                table: "EstadoTareas",
                type: "int",
                nullable: true);

            // Cada tablero recibe copia de los estados base y de los extra que ya usa; sus listas y tareas
            // pasan a esas copias y se borran los estados globales.
            migrationBuilder.Sql("""
                INSERT INTO EstadoTareas (Nombre, Descripcion, TableroId)
                SELECT e.Nombre, e.Descripcion, t.TableroId
                FROM EstadoTareas e CROSS JOIN Tableros t
                WHERE e.TableroId IS NULL AND (
                    e.Nombre IN (N'Pendiente', N'En progreso', N'Finalizada')
                    OR EXISTS (SELECT 1 FROM ListaTareas l WHERE l.TableroId = t.TableroId AND l.EstadoTareaId = e.EstadoTareaId)
                    OR EXISTS (SELECT 1 FROM Tareas ta JOIN ListaTareas l ON l.ListaTareaId = ta.ListaTareaId
                               WHERE l.TableroId = t.TableroId AND ta.EstadoTareaId = e.EstadoTareaId));

                UPDATE l SET EstadoTareaId = n.EstadoTareaId
                FROM ListaTareas l
                JOIN EstadoTareas o ON o.EstadoTareaId = l.EstadoTareaId AND o.TableroId IS NULL
                JOIN EstadoTareas n ON n.TableroId = l.TableroId AND n.Nombre = o.Nombre;

                UPDATE ta SET EstadoTareaId = n.EstadoTareaId
                FROM Tareas ta
                JOIN ListaTareas l ON l.ListaTareaId = ta.ListaTareaId
                JOIN EstadoTareas o ON o.EstadoTareaId = ta.EstadoTareaId AND o.TableroId IS NULL
                JOIN EstadoTareas n ON n.TableroId = l.TableroId AND n.Nombre = o.Nombre;

                DELETE FROM EstadoTareas WHERE TableroId IS NULL;
                """);

            migrationBuilder.AlterColumn<int>(
                name: "TableroId",
                table: "EstadoTareas",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_EstadoTareas_TableroId_Nombre",
                table: "EstadoTareas",
                columns: new[] { "TableroId", "Nombre" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_EstadoTareas_Tableros_TableroId",
                table: "EstadoTareas",
                column: "TableroId",
                principalTable: "Tableros",
                principalColumn: "TableroId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EstadoTareas_Tableros_TableroId",
                table: "EstadoTareas");

            migrationBuilder.DropIndex(
                name: "IX_EstadoTareas_TableroId_Nombre",
                table: "EstadoTareas");

            migrationBuilder.DropColumn(
                name: "TableroId",
                table: "EstadoTareas");

            migrationBuilder.CreateIndex(
                name: "IX_EstadoTareas_Nombre",
                table: "EstadoTareas",
                column: "Nombre",
                unique: true);
        }
    }
}
