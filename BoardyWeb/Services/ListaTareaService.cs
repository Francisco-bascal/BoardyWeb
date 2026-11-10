using BoardyWeb.Comun;
using BoardyWeb.Data;
using BoardyWeb.DTOs;
using BoardyWeb.Models;
using Microsoft.EntityFrameworkCore;

namespace BoardyWeb.Services
{
    public class ListaTareaService : IListaTareaService
    {
        private readonly AppDbContext _context;
        private readonly IAuditoriaService _auditoria;

        public ListaTareaService(AppDbContext context, IAuditoriaService auditoria)
        {
            _context = context;
            _auditoria = auditoria;
        }

        public async Task<IReadOnlyList<ListaTareaDto>> ListarAsync(int tableroId, CancellationToken cancellationToken = default)
        {
            return await _context.ListaTareas
                .AsNoTracking()
                .Where(l => l.TableroId == tableroId)
                .OrderBy(l => l.Orden)
                .Select(l => new ListaTareaDto
                {
                    ListaTareaId = l.ListaTareaId,
                    Nombre = l.Nombre,
                    Descripcion = l.Descripcion,
                    Orden = l.Orden,
                    TableroId = l.TableroId,
                    EstadoTareaId = l.EstadoTareaId,
                    EstadoNombre = l.EstadoTarea != null ? l.EstadoTarea.Nombre : string.Empty
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<int?> ObtenerTableroIdAsync(int listaTareaId, CancellationToken cancellationToken = default)
        {
            return await _context.ListaTareas
                .AsNoTracking()
                .Where(l => l.ListaTareaId == listaTareaId)
                .Select(l => (int?)l.TableroId)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<int?> ObtenerEstadoAsync(int listaTareaId, CancellationToken cancellationToken = default)
        {
            return await _context.ListaTareas
                .AsNoTracking()
                .Where(l => l.ListaTareaId == listaTareaId)
                .Select(l => l.EstadoTareaId)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<ResultadoOperacion<ListaTareaDto>> CrearAsync(int tableroId, CrearListaTareaDto dto, CancellationToken cancellationToken = default)
        {
            var tablero = await _context.Tableros
                .Where(t => t.TableroId == tableroId)
                .Select(t => new { t.EsKanban })
                .FirstOrDefaultAsync(cancellationToken);

            if (tablero is null)
            {
                return ResultadoOperacion<ListaTareaDto>.NoExiste("El tablero no existe.");
            }

            var (estadoTareaId, error) = await ResolverEstadoAsync(tablero.EsKanban, tableroId, null, dto.EstadoTareaId, cancellationToken);
            if (error is not null)
            {
                return ResultadoOperacion<ListaTareaDto>.Fallo(nameof(dto.EstadoTareaId), error);
            }

            var ultimoOrden = await _context.ListaTareas
                .Where(l => l.TableroId == tableroId)
                .MaxAsync(l => (int?)l.Orden, cancellationToken) ?? 0;

            var lista = new ListaTarea
            {
                TableroId = tableroId,
                Nombre = dto.Nombre.Trim(),
                Descripcion = dto.Descripcion.Trim(),
                Orden = ultimoOrden + 1,
                EstadoTareaId = estadoTareaId
            };

            _context.ListaTareas.Add(lista);
            _auditoria.Registrar(tableroId, $"Creó la lista \"{lista.Nombre}\".");
            await _context.SaveChangesAsync(cancellationToken);

            return ResultadoOperacion<ListaTareaDto>.Exitoso(Mapear(lista), "Lista creada correctamente.");
        }

        public async Task<ResultadoOperacion<ListaTareaDto>> ActualizarAsync(int listaTareaId, ActualizarListaTareaDto dto, CancellationToken cancellationToken = default)
        {
            var lista = await _context.ListaTareas
                .Include(l => l.Tablero)
                .FirstOrDefaultAsync(l => l.ListaTareaId == listaTareaId, cancellationToken);

            if (lista is null)
            {
                return ResultadoOperacion<ListaTareaDto>.NoExiste("La lista no existe.");
            }

            var (estadoTareaId, error) = await ResolverEstadoAsync(lista.Tablero.EsKanban, lista.TableroId, listaTareaId, dto.EstadoTareaId, cancellationToken);
            if (error is not null)
            {
                return ResultadoOperacion<ListaTareaDto>.Fallo(nameof(dto.EstadoTareaId), error);
            }

            // Regla de sincronización Kanban: si cambia el estado de la lista, cambia el de sus tareas.
            if (estadoTareaId is not null && estadoTareaId != lista.EstadoTareaId)
            {
                var tareas = await _context.Tareas.Where(t => t.ListaTareaId == listaTareaId).ToListAsync(cancellationToken);
                tareas.ForEach(t => t.EstadoTareaId = estadoTareaId.Value);
            }

            lista.Nombre = dto.Nombre.Trim();
            lista.Descripcion = dto.Descripcion.Trim();
            lista.EstadoTareaId = estadoTareaId;
            _auditoria.Registrar(lista.TableroId, $"Editó la lista \"{lista.Nombre}\".");
            await _context.SaveChangesAsync(cancellationToken);

            return ResultadoOperacion<ListaTareaDto>.Exitoso(Mapear(lista), "Lista actualizada correctamente.");
        }

        public async Task<ResultadoOperacion> ReordenarAsync(int listaTareaId, ReordenarDto dto, CancellationToken cancellationToken = default)
        {
            var lista = await _context.ListaTareas.FirstOrDefaultAsync(l => l.ListaTareaId == listaTareaId, cancellationToken);

            if (lista is null)
            {
                return ResultadoOperacion.NoExiste("La lista no existe.");
            }

            var listas = await _context.ListaTareas
                .Where(l => l.TableroId == lista.TableroId)
                .OrderBy(l => l.Orden)
                .ToListAsync(cancellationToken);

            listas.Remove(lista);
            listas.Insert(Math.Min(dto.NuevoOrden, listas.Count + 1) - 1, lista);

            for (var i = 0; i < listas.Count; i++)
            {
                listas[i].Orden = i + 1;
            }

            _auditoria.Registrar(lista.TableroId, $"Reordenó la lista \"{lista.Nombre}\".");
            await _context.SaveChangesAsync(cancellationToken);

            return ResultadoOperacion.Exitoso("Lista reordenada correctamente.");
        }

        public async Task<ResultadoOperacion> EliminarAsync(int listaTareaId, CancellationToken cancellationToken = default)
        {
            var lista = await _context.ListaTareas.FirstOrDefaultAsync(l => l.ListaTareaId == listaTareaId, cancellationToken);

            if (lista is null)
            {
                return ResultadoOperacion.NoExiste("La lista no existe.");
            }

            // No eliminación de listas con tareas: primero hay que vaciarla.
            if (await _context.Tareas.AnyAsync(t => t.ListaTareaId == listaTareaId, cancellationToken))
            {
                return ResultadoOperacion.Fallo("No se puede eliminar una lista que contiene tareas.");
            }

            _context.ListaTareas.Remove(lista);
            _auditoria.Registrar(lista.TableroId, $"Eliminó la lista \"{lista.Nombre}\".");
            await _context.SaveChangesAsync(cancellationToken);

            return ResultadoOperacion.Exitoso("Lista eliminada correctamente.");
        }

        // En Kanban el estado es obligatorio y único por tablero; en esquema libre las listas nunca tienen estado.
        private async Task<(int? EstadoTareaId, string? Error)> ResolverEstadoAsync(bool? esKanban, int tableroId, int? listaTareaId, int? estadoTareaId, CancellationToken cancellationToken)
        {
            if (esKanban != true)
            {
                return (null, null);
            }

            // Regla de estado obligatorio: en Kanban cada lista necesita un estado válido.
            if (estadoTareaId is null || !await _context.EstadoTareas.AnyAsync(e => e.EstadoTareaId == estadoTareaId && e.TableroId == tableroId, cancellationToken))
            {
                return (null, "Debés seleccionar un estado.");
            }

            // Regla de estado único: no puede haber dos listas con el mismo estado en el tablero.
            if (await _context.ListaTareas.AnyAsync(l => l.TableroId == tableroId && l.EstadoTareaId == estadoTareaId && l.ListaTareaId != listaTareaId, cancellationToken))
            {
                return (null, "Ya existe una lista para ese estado.");
            }

            return (estadoTareaId, null);
        }

        private static ListaTareaDto Mapear(ListaTarea lista) => new()
        {
            ListaTareaId = lista.ListaTareaId,
            Nombre = lista.Nombre,
            Descripcion = lista.Descripcion,
            Orden = lista.Orden,
            TableroId = lista.TableroId,
            EstadoTareaId = lista.EstadoTareaId
        };
    }
}
