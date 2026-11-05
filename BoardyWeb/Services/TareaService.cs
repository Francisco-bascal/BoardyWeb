using System.Linq.Expressions;
using BoardyWeb.Comun;
using BoardyWeb.Data;
using BoardyWeb.DTOs;
using BoardyWeb.Models;
using Microsoft.EntityFrameworkCore;

namespace BoardyWeb.Services
{
    public class TareaService : ITareaService
    {
        private readonly AppDbContext _context;
        private readonly IAuditoriaService _auditoria;

        public TareaService(AppDbContext context, IAuditoriaService auditoria)
        {
            _context = context;
            _auditoria = auditoria;
        }

        public async Task<IReadOnlyList<TareaDto>> ListarPorTableroAsync(int tableroId, CancellationToken cancellationToken = default)
        {
            return await _context.Tareas
                .AsNoTracking()
                .Where(t => t.ListaTarea.TableroId == tableroId)
                .OrderBy(t => t.ListaTarea.Orden)
                .ThenBy(t => t.Orden)
                .Select(Proyectar())
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<TareaDto>> ListarAsignadasAsync(string usuarioId, CancellationToken cancellationToken = default)
        {
            return await _context.Tareas
                .AsNoTracking()
                .Where(t => t.MiembrosAsignados.Any(m => m.TableroMiembro.UsuarioId == usuarioId)
                    && t.EstadoTarea.Nombre != EstadosTareaSistema.Finalizada)
                .OrderBy(t => t.ListaTarea.Tablero.Nombre)
                .ThenBy(t => t.FechaVencimiento == null)
                .ThenBy(t => t.FechaVencimiento)
                .Select(Proyectar())
                .ToListAsync(cancellationToken);
        }

        public async Task<ResultadoOperacion<TareaDto>> ObtenerAsync(int tareaId, CancellationToken cancellationToken = default)
        {
            var tarea = await ConsultarDto(tareaId, cancellationToken);

            return tarea is null
                ? ResultadoOperacion<TareaDto>.NoExiste("La tarea no existe.")
                : ResultadoOperacion<TareaDto>.Exitoso(tarea);
        }

        public async Task<int?> ObtenerTableroIdAsync(int tareaId, CancellationToken cancellationToken = default)
        {
            return await _context.Tareas
                .AsNoTracking()
                .Where(t => t.TareaId == tareaId)
                .Select(t => (int?)t.ListaTarea.TableroId)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<ResultadoOperacion<TareaDto>> CrearAsync(int listaTareaId, CrearTareaDto dto, CancellationToken cancellationToken = default)
        {
            var lista = await _context.ListaTareas
                .Where(l => l.ListaTareaId == listaTareaId)
                .Select(l => new { l.EstadoTareaId, l.TableroId })
                .FirstOrDefaultAsync(cancellationToken);

            if (lista is null)
            {
                return ResultadoOperacion<TareaDto>.NoExiste("La lista no existe.");
            }

            // Si la lista tiene estado (Kanban) manda la lista, no el formulario.
            var estadoTareaId = lista.EstadoTareaId ?? dto.EstadoTareaId;

            if (!await _context.EstadoTareas.AnyAsync(e => e.EstadoTareaId == estadoTareaId, cancellationToken))
            {
                return ResultadoOperacion<TareaDto>.Fallo(nameof(dto.EstadoTareaId), "El estado seleccionado no existe.");
            }

            var ultimoOrden = await _context.Tareas
                .Where(t => t.ListaTareaId == listaTareaId)
                .MaxAsync(t => (int?)t.Orden, cancellationToken) ?? 0;

            var tarea = new Tarea
            {
                ListaTareaId = listaTareaId,
                Titulo = dto.Titulo.Trim(),
                Descripcion = dto.Descripcion.Trim(),
                FechaVencimiento = dto.FechaVencimiento,
                Prioridad = dto.Prioridad,
                EstadoTareaId = estadoTareaId,
                Orden = ultimoOrden + 1
            };

            _context.Tareas.Add(tarea);
            _auditoria.Registrar(lista.TableroId, $"Creó la tarea \"{tarea.Titulo}\".");
            await _context.SaveChangesAsync(cancellationToken);

            return ResultadoOperacion<TareaDto>.Exitoso((await ConsultarDto(tarea.TareaId, cancellationToken))!, "Tarea creada correctamente.");
        }

        public async Task<ResultadoOperacion<TareaDto>> ActualizarAsync(int tareaId, ActualizarTareaDto dto, CancellationToken cancellationToken = default)
        {
            var tarea = await _context.Tareas
                .Include(t => t.ListaTarea)
                .FirstOrDefaultAsync(t => t.TareaId == tareaId, cancellationToken);

            if (tarea is null)
            {
                return ResultadoOperacion<TareaDto>.NoExiste("La tarea no existe.");
            }

            var estadoTareaId = tarea.ListaTarea.EstadoTareaId ?? dto.EstadoTareaId;

            if (!await _context.EstadoTareas.AnyAsync(e => e.EstadoTareaId == estadoTareaId, cancellationToken))
            {
                return ResultadoOperacion<TareaDto>.Fallo(nameof(dto.EstadoTareaId), "El estado seleccionado no existe.");
            }

            tarea.Titulo = dto.Titulo.Trim();
            tarea.Descripcion = dto.Descripcion.Trim();
            tarea.FechaVencimiento = dto.FechaVencimiento;
            tarea.Prioridad = dto.Prioridad;
            tarea.EstadoTareaId = estadoTareaId;
            _auditoria.Registrar(tarea.ListaTarea.TableroId, $"Editó la tarea \"{tarea.Titulo}\".");
            await _context.SaveChangesAsync(cancellationToken);

            return ResultadoOperacion<TareaDto>.Exitoso((await ConsultarDto(tareaId, cancellationToken))!, "Tarea actualizada correctamente.");
        }

        public async Task<ResultadoOperacion<TareaDto>> MoverAsync(int tareaId, MoverTareaDto dto, CancellationToken cancellationToken = default)
        {
            var tarea = await _context.Tareas
                .Include(t => t.ListaTarea)
                .FirstOrDefaultAsync(t => t.TareaId == tareaId, cancellationToken);

            if (tarea is null)
            {
                return ResultadoOperacion<TareaDto>.NoExiste("La tarea no existe.");
            }

            var listaDestino = await _context.ListaTareas
                .AsNoTracking()
                .FirstOrDefaultAsync(l => l.ListaTareaId == dto.ListaTareaId, cancellationToken);

            if (listaDestino is null)
            {
                return ResultadoOperacion<TareaDto>.NoExiste("La lista de destino no existe.");
            }

            if (listaDestino.TableroId != tarea.ListaTarea.TableroId)
            {
                return ResultadoOperacion<TareaDto>.Fallo(nameof(dto.ListaTareaId), "La lista de destino pertenece a otro tablero.");
            }

            var listaOrigenId = tarea.ListaTareaId;

            var tareasDestino = await _context.Tareas
                .Where(t => t.ListaTareaId == dto.ListaTareaId && t.TareaId != tareaId)
                .OrderBy(t => t.Orden)
                .ToListAsync(cancellationToken);

            var posicion = Math.Min(dto.NuevoOrden ?? tareasDestino.Count + 1, tareasDestino.Count + 1);
            tareasDestino.Insert(posicion - 1, tarea);
            tarea.ListaTareaId = dto.ListaTareaId;
            tarea.EstadoTareaId = listaDestino.EstadoTareaId ?? tarea.EstadoTareaId;
            Renumerar(tareasDestino);

            if (listaOrigenId != dto.ListaTareaId)
            {
                var tareasOrigen = await _context.Tareas
                    .Where(t => t.ListaTareaId == listaOrigenId && t.TareaId != tareaId)
                    .OrderBy(t => t.Orden)
                    .ToListAsync(cancellationToken);

                Renumerar(tareasOrigen);
            }

            _auditoria.Registrar(listaDestino.TableroId, $"Movió la tarea \"{tarea.Titulo}\" a \"{listaDestino.Nombre}\".");
            await _context.SaveChangesAsync(cancellationToken);

            return ResultadoOperacion<TareaDto>.Exitoso((await ConsultarDto(tareaId, cancellationToken))!, "Tarea movida correctamente.");
        }

        public async Task<ResultadoOperacion<TareaDto>> CambiarEstadoAsync(int tareaId, CambiarEstadoTareaDto dto, CancellationToken cancellationToken = default)
        {
            var tarea = await _context.Tareas
                .Include(t => t.ListaTarea)
                .FirstOrDefaultAsync(t => t.TareaId == tareaId, cancellationToken);

            if (tarea is null)
            {
                return ResultadoOperacion<TareaDto>.NoExiste("La tarea no existe.");
            }

            // En Kanban cambiar el estado es mover la tarea a una lista con ese estado.
            if (tarea.ListaTarea.EstadoTareaId is not null)
            {
                var listaDestinoId = await _context.ListaTareas
                    .Where(l => l.TableroId == tarea.ListaTarea.TableroId && l.EstadoTareaId == dto.EstadoTareaId)
                    .OrderBy(l => l.Orden)
                    .Select(l => (int?)l.ListaTareaId)
                    .FirstOrDefaultAsync(cancellationToken);

                return listaDestinoId is null
                    ? ResultadoOperacion<TareaDto>.Fallo(nameof(dto.EstadoTareaId), "El tablero no tiene una lista con ese estado.")
                    : await MoverAsync(tareaId, new MoverTareaDto { ListaTareaId = listaDestinoId.Value }, cancellationToken);
            }

            if (!await _context.EstadoTareas.AnyAsync(e => e.EstadoTareaId == dto.EstadoTareaId, cancellationToken))
            {
                return ResultadoOperacion<TareaDto>.Fallo(nameof(dto.EstadoTareaId), "El estado seleccionado no existe.");
            }

            tarea.EstadoTareaId = dto.EstadoTareaId;
            var nombreEstado = await _context.EstadoTareas.Where(e => e.EstadoTareaId == dto.EstadoTareaId).Select(e => e.Nombre).FirstAsync(cancellationToken);
            _auditoria.Registrar(tarea.ListaTarea.TableroId, $"Cambió el estado de la tarea \"{tarea.Titulo}\" a \"{nombreEstado}\".");
            await _context.SaveChangesAsync(cancellationToken);

            return ResultadoOperacion<TareaDto>.Exitoso((await ConsultarDto(tareaId, cancellationToken))!, "Estado actualizado correctamente.");
        }

        public async Task<ResultadoOperacion<TareaDto>> AsignarMiembroAsync(int tareaId, int tableroMiembroId, CancellationToken cancellationToken = default)
        {
            var tableroId = await ObtenerTableroIdAsync(tareaId, cancellationToken);

            if (tableroId is null)
            {
                return ResultadoOperacion<TareaDto>.NoExiste("La tarea no existe.");
            }

            if (!await _context.TableroMiembros.AnyAsync(m => m.TableroMiembroId == tableroMiembroId && m.TableroId == tableroId, cancellationToken))
            {
                return ResultadoOperacion<TareaDto>.Fallo("El miembro no pertenece al tablero de la tarea.");
            }

            if (await _context.TareaMiembros.AnyAsync(m => m.TareaId == tareaId && m.TableroMiembroId == tableroMiembroId, cancellationToken))
            {
                return ResultadoOperacion<TareaDto>.Fallo("El miembro ya está asignado a la tarea.");
            }

            _context.TareaMiembros.Add(new TareaMiembro
            {
                TareaId = tareaId,
                TableroMiembroId = tableroMiembroId,
                FechaAsignacion = DateTime.UtcNow
            });
            var datos = await _context.TableroMiembros.Where(m => m.TableroMiembroId == tableroMiembroId).Select(m => new { Nombre = m.Usuario.Nombre + " " + m.Usuario.Apellido, Titulo = _context.Tareas.Where(t => t.TareaId == tareaId).Select(t => t.Titulo).First() }).FirstAsync(cancellationToken);
            _auditoria.Registrar(tableroId.Value, $"Asignó a {datos.Nombre} a la tarea \"{datos.Titulo}\".");
            await _context.SaveChangesAsync(cancellationToken);

            return ResultadoOperacion<TareaDto>.Exitoso((await ConsultarDto(tareaId, cancellationToken))!, "Miembro asignado correctamente.");
        }

        public async Task<ResultadoOperacion<TareaDto>> QuitarMiembroAsync(int tareaId, int tableroMiembroId, CancellationToken cancellationToken = default)
        {
            var asignacion = await _context.TareaMiembros
                .FirstOrDefaultAsync(m => m.TareaId == tareaId && m.TableroMiembroId == tableroMiembroId, cancellationToken);

            if (asignacion is null)
            {
                return ResultadoOperacion<TareaDto>.NoExiste("El miembro no está asignado a la tarea.");
            }

            _context.TareaMiembros.Remove(asignacion);
            var datos = await _context.TareaMiembros.Where(m => m.TareaMiembroId == asignacion.TareaMiembroId).Select(m => new { m.Tarea.ListaTarea.TableroId, m.Tarea.Titulo, Nombre = m.TableroMiembro.Usuario.Nombre + " " + m.TableroMiembro.Usuario.Apellido }).FirstAsync(cancellationToken);
            _auditoria.Registrar(datos.TableroId, $"Quitó a {datos.Nombre} de la tarea \"{datos.Titulo}\".");
            await _context.SaveChangesAsync(cancellationToken);

            return ResultadoOperacion<TareaDto>.Exitoso((await ConsultarDto(tareaId, cancellationToken))!, "Miembro quitado correctamente.");
        }

        public async Task<ResultadoOperacion> EliminarAsync(int tareaId, CancellationToken cancellationToken = default)
        {
            var tarea = await _context.Tareas.FirstOrDefaultAsync(t => t.TareaId == tareaId, cancellationToken);

            if (tarea is null)
            {
                return ResultadoOperacion.NoExiste("La tarea no existe.");
            }

            var listaTareaId = tarea.ListaTareaId;
            _context.Tareas.Remove(tarea);

            var restantes = await _context.Tareas
                .Where(t => t.ListaTareaId == listaTareaId && t.TareaId != tareaId)
                .OrderBy(t => t.Orden)
                .ToListAsync(cancellationToken);

            Renumerar(restantes);
            var tableroId = await ObtenerTableroIdAsync(tareaId, cancellationToken);
            _auditoria.Registrar(tableroId!.Value, $"Eliminó la tarea \"{tarea.Titulo}\".");
            await _context.SaveChangesAsync(cancellationToken);

            return ResultadoOperacion.Exitoso("Tarea eliminada correctamente.");
        }

        private static void Renumerar(IReadOnlyList<Tarea> tareas)
        {
            for (var i = 0; i < tareas.Count; i++)
            {
                tareas[i].Orden = i + 1;
            }
        }

        private Task<TareaDto?> ConsultarDto(int tareaId, CancellationToken cancellationToken)
        {
            return _context.Tareas
                .AsNoTracking()
                .Where(t => t.TareaId == tareaId)
                .Select(Proyectar())
                .FirstOrDefaultAsync(cancellationToken);
        }

        private static Expression<Func<Tarea, TareaDto>> Proyectar() =>
            t => new TareaDto
            {
                TareaId = t.TareaId,
                Titulo = t.Titulo,
                Descripcion = t.Descripcion,
                FechaVencimiento = t.FechaVencimiento,
                Prioridad = t.Prioridad,
                Orden = t.Orden,
                ListaTareaId = t.ListaTareaId,
                ListaNombre = t.ListaTarea.Nombre,
                TableroId = t.ListaTarea.TableroId,
                TableroNombre = t.ListaTarea.Tablero.Nombre,
                EstadoTareaId = t.EstadoTareaId,
                EstadoNombre = t.EstadoTarea.Nombre,
                Etiquetas = t.Etiquetas
                    .Select(te => new EtiquetaDto
                    {
                        EtiquetaId = te.EtiquetaId,
                        Nombre = te.Etiqueta.Nombre,
                        Descripcion = te.Etiqueta.Descripcion,
                        Color = te.Etiqueta.Color
                    })
                    .ToList(),
                Miembros = t.MiembrosAsignados
                    .Select(ma => new TableroMiembroDto
                    {
                        TableroMiembroId = ma.TableroMiembroId,
                        UsuarioId = ma.TableroMiembro.UsuarioId,
                        NombreCompleto = ma.TableroMiembro.Usuario.Nombre + " " + ma.TableroMiembro.Usuario.Apellido,
                        Email = ma.TableroMiembro.Usuario.Email ?? string.Empty,
                        EsPropietario = ma.TableroMiembro.Tablero.PropietarioId == ma.TableroMiembro.UsuarioId,
                        FotoPerfil = ma.TableroMiembro.Usuario.FotoPerfil
                    })
                    .ToList()
            };
    }
}
