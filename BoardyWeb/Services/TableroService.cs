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
        private readonly IAuditoriaService _auditoria;

        public TableroService(AppDbContext context, IAuditoriaService auditoria)
        {
            _context = context;
            _auditoria = auditoria;
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
                    EsPropietario = t.PropietarioId == usuarioId,
                    EsKanban = t.EsKanban == true
                })
                .FirstOrDefaultAsync(cancellationToken);

            return tablero is null
                ? ResultadoOperacion<TableroDetalleDto>.NoExiste("El tablero no existe.")
                : ResultadoOperacion<TableroDetalleDto>.Exitoso(tablero);
        }

        public async Task<bool> EsKanbanAsync(int tableroId, CancellationToken cancellationToken = default)
        {
            return await _context.Tableros.AnyAsync(t => t.TableroId == tableroId && t.EsKanban == true, cancellationToken);
        }

        public async Task<ResultadoOperacion<TableroDto>> CrearAsync(CrearTableroDto dto, string usuarioId, CancellationToken cancellationToken = default)
        {
            var rolPropietarioId = await _context.RolMiembros
                .Where(r => r.Nombre == RolesTablero.Propietario)
                .Select(r => (int?)r.RolMiembroId)
                .FirstOrDefaultAsync(cancellationToken);

            // Regla de propietario inicial: quien crea el tablero es su Propietario, y el rol debe existir.
            if (rolPropietarioId is null)
            {
                return ResultadoOperacion<TableroDto>.Fallo("No está configurado el rol Propietario.");
            }

            var ahora = DateTime.UtcNow;
            var tablero = new Tablero
            {
                Nombre = dto.Nombre.Trim(),
                Descripcion = dto.Descripcion.Trim(),
                PropietarioId = usuarioId,
                EsKanban = dto.EsKanban
            };

            // Regla de estados base: todo tablero nace con sus 3 estados inmutables.
            var estados = EstadosTareaSistema.CrearBase();
            estados.ForEach(tablero.EstadosTarea.Add);

            // Regla de esquema Kanban: arranca con una lista por cada estado base.
            if (dto.EsKanban)
            {
                var orden = 1;
                foreach (var estado in estados)
                {
                    tablero.ListasTarea.Add(new ListaTarea { Nombre = estado.Nombre, Orden = orden++, EstadoTarea = estado });
                }
            }

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

            // El id del tablero recién existe tras el primer guardado.
            _auditoria.Registrar(tablero.TableroId, $"Creó el tablero \"{tablero.Nombre}\".");
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
            _auditoria.Registrar(tableroId, $"Editó el tablero \"{tablero.Nombre}\".");
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

        public async Task<ResultadoOperacion> TransferirPropiedadAsync(int tableroId, int tableroMiembroDestinoId, string usuarioActualId, CancellationToken cancellationToken = default)
        {
            var tablero = await _context.Tableros.FirstOrDefaultAsync(t => t.TableroId == tableroId, cancellationToken);

            if (tablero is null)
            {
                return ResultadoOperacion.NoExiste("El tablero no existe.");
            }

            // Regla de transferencia: solo el propietario puede ceder la propiedad.
            if (tablero.PropietarioId != usuarioActualId)
            {
                return ResultadoOperacion.Prohibido("Solo el propietario puede transferir la propiedad del tablero.");
            }

            var destino = await _context.TableroMiembros
                .Include(m => m.Roles).ThenInclude(r => r.RolMiembro)
                .FirstOrDefaultAsync(m => m.TableroMiembroId == tableroMiembroDestinoId && m.TableroId == tableroId, cancellationToken);

            // Regla de destino miembro: debe pertenecer al tablero.
            if (destino is null)
            {
                return ResultadoOperacion.NoExiste("El miembro no pertenece al tablero.");
            }

            // No transferencia a sí mismo: el destino no puede ser ya el propietario.
            if (destino.UsuarioId == tablero.PropietarioId)
            {
                return ResultadoOperacion.Fallo("Ese usuario ya es el propietario del tablero.");
            }

            // Regla de destino Administrador: solo se transfiere a un miembro con ese rol.
            if (!destino.Roles.Any(r => r.RolMiembro.Nombre == RolesTablero.Administrador))
            {
                return ResultadoOperacion.Fallo("Solo se puede transferir la propiedad a un miembro con el rol Administrador.");
            }

            var rolPropietario = await _context.RolMiembros.FirstOrDefaultAsync(r => r.Nombre == RolesTablero.Propietario, cancellationToken);
            var rolAdministrador = await _context.RolMiembros.FirstOrDefaultAsync(r => r.Nombre == RolesTablero.Administrador, cancellationToken);

            if (rolPropietario is null || rolAdministrador is null)
            {
                return ResultadoOperacion.Fallo("No están configurados los roles Propietario y Administrador.");
            }

            var actual = await _context.TableroMiembros
                .Include(m => m.Roles)
                .FirstAsync(m => m.TableroId == tableroId && m.UsuarioId == usuarioActualId, cancellationToken);

            var ahora = DateTime.UtcNow;

            // Cada miembro tiene un único rol. Todo se persiste en un único SaveChanges, de modo que
            // nunca queda un tablero sin propietario: el nuevo propietario pasa a Propietario y el
            // anterior a Administrador.
            tablero.PropietarioId = destino.UsuarioId;

            _context.MiembrosPorRol.RemoveRange(destino.Roles);
            destino.Roles.Add(new MiembrosPorRol { RolMiembroId = rolPropietario.RolMiembroId, FechaAsignacion = ahora });

            _context.MiembrosPorRol.RemoveRange(actual.Roles);
            actual.Roles.Add(new MiembrosPorRol { RolMiembroId = rolAdministrador.RolMiembroId, FechaAsignacion = ahora });

            var nombreDestino = await _context.Users.Where(u => u.Id == destino.UsuarioId).Select(u => u.Nombre + " " + u.Apellido).FirstAsync(cancellationToken);
            _auditoria.Registrar(tableroId, $"Transfirió la propiedad del tablero a {nombreDestino}.");
            await _context.SaveChangesAsync(cancellationToken);

            return ResultadoOperacion.Exitoso("La propiedad del tablero fue transferida. Ahora tenés el rol Administrador.");
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
