using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoardyWeb.Migrations
{
    /// <inheritdoc />
    public partial class UnRolPorMiembro : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Normaliza los datos existentes antes de exigir un único rol por miembro:
            // 1) el propietario conserva solo el rol Propietario;
            // 2) los demás miembros conservan el rol asignado más recientemente.
            migrationBuilder.Sql(@"
                DELETE mr
                  FROM MiembrosPorRol mr
                  JOIN TableroMiembros tm ON tm.TableroMiembroId = mr.TableroMiembroId
                  JOIN Tableros t ON t.TableroId = tm.TableroId AND t.PropietarioId = tm.UsuarioId
                  JOIN RolMiembros r ON r.RolMiembroId = mr.RolMiembroId
                 WHERE r.Nombre <> 'Propietario'
                   AND EXISTS (SELECT 1 FROM MiembrosPorRol p
                                 JOIN RolMiembros rp ON rp.RolMiembroId = p.RolMiembroId
                                WHERE p.TableroMiembroId = mr.TableroMiembroId AND rp.Nombre = 'Propietario');");

            migrationBuilder.Sql(@"
                WITH ordenadas AS (
                    SELECT MiembroPorRolId,
                           ROW_NUMBER() OVER (PARTITION BY TableroMiembroId
                                              ORDER BY FechaAsignacion DESC, MiembroPorRolId DESC) AS n
                      FROM MiembrosPorRol)
                DELETE FROM ordenadas WHERE n > 1;");

            migrationBuilder.DropIndex(
                name: "IX_MiembrosPorRol_TableroMiembroId_RolMiembroId",
                table: "MiembrosPorRol");

            migrationBuilder.CreateIndex(
                name: "IX_MiembrosPorRol_TableroMiembroId",
                table: "MiembrosPorRol",
                column: "TableroMiembroId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MiembrosPorRol_TableroMiembroId",
                table: "MiembrosPorRol");

            migrationBuilder.CreateIndex(
                name: "IX_MiembrosPorRol_TableroMiembroId_RolMiembroId",
                table: "MiembrosPorRol",
                columns: new[] { "TableroMiembroId", "RolMiembroId" },
                unique: true);
        }
    }
}
