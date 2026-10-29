using BoardyWeb.Autorizacion;
using BoardyWeb.Comun;
using BoardyWeb.Data;
using BoardyWeb.DTOs;
using BoardyWeb.Models;
using Microsoft.EntityFrameworkCore;

namespace BoardyWeb.Services
{
    public class TableroService : ITableroService
    {
        private readonly AppDbContext _context;

        public TableroService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<TableroDto>> ListarDelUsuarioAsync(string usuarioId, CancellationToken cancellationToken = default)
        {
            return await _context.Tableros
                .AsNoTracking()
                .Where(t => t.Miembros.Any(m => m.UsuarioId == usuarioId))
                .OrderBy(t => t.Nombre)
                .Select(t => new TableroDto
                {
                    TableroId = t.TableroId,
                    Nombre = t.Nombre,
                    Descripcion = t.Descripcion,
                    FechaCreacion = t.FechaCreacion
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<ResultadoOperacion<TableroDetalleDto>> ObtenerDetalleAsync(int tableroId, string usuarioId, CancellationToken cancellationToken = default)
        {
            var tablero = await _context.Tableros
                .AsNoTracking()
                .Where(t => t.TableroId == tableroId)
                .Select(t => new TableroDetalleDto
                {
                    TableroId = t.TableroId,
                    Nombre = t.Nombre,
                    Descripcion = t.Descripcion,
                    PropietarioNombre = t.Propietario.Nombre + " " + t.Propietario.Apellido,
                    EsPropietario = t.PropietarioId == usuarioId
                })
                .FirstOrDefaultAsync(cancellationToken);

            return tablero is null
                ? ResultadoOperacion<TableroDetalleDto>.NoExiste("El tablero no existe.")
                : ResultadoOperacion<TableroDetalleDto>.Exitoso(tablero);
        }

        public async Task<ResultadoOperacion<TableroDto>> CrearAsync(CrearTableroDto dto, string usuarioId, CancellationToken cancellationToken = default)
        {
            var rolPropietarioId = await _context.RolMiembros
                .Where(r => r.Nombre == RolesTablero.Propietario)
                .Select(r => (int?)r.RolMiembroId)
                .FirstOrDefaultAsync(cancellationToken);

            if (rolPropietarioId is null)
            {
                return ResultadoOperacion<TableroDto>.Fallo("No está configurado el rol Propietario.");
            }

            var ahora = DateTime.UtcNow;
            var tablero = new Tablero
            {
                Nombre = dto.Nombre.Trim(),
                Descripcion = dto.Descripcion.Trim(),
                PropietarioId = usuarioId
            };

            tablero.Miembros.Add(new TableroMiembro
            {
                UsuarioId = usuarioId,
                FechaAsignacion = ahora,
                Roles = new List<MiembrosPorRol>
                {
                    new() { RolMiembroId = rolPropietarioId.Value, FechaAsignacion = ahora }
                }
            });

            _context.Tableros.Add(tablero);
            await _context.SaveChangesAsync(cancellationToken);

            return ResultadoOperacion<TableroDto>.Exitoso(Mapear(tablero), "Tablero creado correctamente.");
        }

        public async Task<ResultadoOperacion<TableroDto>> ActualizarAsync(int tableroId, ActualizarTableroDto dto, CancellationToken cancellationToken = default)
        {
            var tablero = await _context.Tableros.FirstOrDefaultAsync(t => t.TableroId == tableroId, cancellationToken);

            if (tablero is null)
            {
                return ResultadoOperacion<TableroDto>.NoExiste("El tablero no existe.");
            }

            tablero.Nombre = dto.Nombre.Trim();
            tablero.Descripcion = dto.Descripcion.Trim();
            await _context.SaveChangesAsync(cancellationToken);

            return ResultadoOperacion<TableroDto>.Exitoso(Mapear(tablero), "Tablero actualizado correctamente.");
        }

        public async Task<ResultadoOperacion> EliminarAsync(int tableroId, CancellationToken cancellationToken = default)
        {
            var tablero = await _context.Tableros.FirstOrDefaultAsync(t => t.TableroId == tableroId, cancellationToken);

            if (tablero is null)
            {
                return ResultadoOperacion.NoExiste("El tablero no existe.");
            }

            // Listas, tareas, miembros, etiquetas e invitaciones se eliminan en cascada en la base de datos.
            _context.Tableros.Remove(tablero);
            await _context.SaveChangesAsync(cancellationToken);

            return ResultadoOperacion.Exitoso("Tablero eliminado correctamente.");
        }

        private static TableroDto Mapear(Tablero tablero) => new()
        {
            TableroId = tablero.TableroId,
            Nombre = tablero.Nombre,
            Descripcion = tablero.Descripcion,
            FechaCreacion = tablero.FechaCreacion
        };
    }
}
