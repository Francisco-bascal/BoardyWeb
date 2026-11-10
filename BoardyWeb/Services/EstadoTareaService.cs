using BoardyWeb.Comun;
using BoardyWeb.Data;
using BoardyWeb.DTOs;
using BoardyWeb.Models;
using Microsoft.EntityFrameworkCore;

namespace BoardyWeb.Services
{
    /// <summary>
    /// Estados de tarea de cada tablero: los 3 base son inmutables y se pueden sumar estados propios.
    /// </summary>
    public class EstadoTareaService : IEstadoTareaService
    {
        private readonly AppDbContext _context;
        private readonly IAuditoriaService _auditoria;

        public EstadoTareaService(AppDbContext context, IAuditoriaService auditoria)
        {
            _context = context;
            _auditoria = auditoria;
        }

        // Estados del tablero, con los base primero por orden de creación.
        public async Task<IReadOnlyList<EstadoTareaDto>> ListarAsync(int tableroId, CancellationToken cancellationToken = default)
        {
            return await _context.EstadoTareas
                .AsNoTracking()
                .Where(e => e.TableroId == tableroId)
                .OrderBy(e => e.EstadoTareaId)
                .Select(e => new EstadoTareaDto
                {
                    EstadoTareaId = e.EstadoTareaId,
                    Nombre = e.Nombre,
                    Descripcion = e.Descripcion,
                    CantidadTareas = e.Tareas.Count,
                    EsBase = EstadosTareaSistema.Todos.Contains(e.Nombre)
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<int?> ObtenerTableroIdAsync(int estadoTareaId, CancellationToken cancellationToken = default)
        {
            return await _context.EstadoTareas
                .AsNoTracking()
                .Where(e => e.EstadoTareaId == estadoTareaId)
                .Select(e => (int?)e.TableroId)
                .FirstOrDefaultAsync(cancellationToken);
        }

        // Cantidad de estados que todavía no tienen lista: en Kanban, cuántas listas más se pueden crear.
        public async Task<int> ContarSinListaAsync(int tableroId, CancellationToken cancellationToken = default)
        {
            return await _context.EstadoTareas.CountAsync(e => e.TableroId == tableroId && !e.Listas.Any(), cancellationToken);
        }

        public async Task<ResultadoOperacion<EstadoTareaDto>> ObtenerAsync(int estadoTareaId, CancellationToken cancellationToken = default)
        {
            var estado = await _context.EstadoTareas.AsNoTracking().FirstOrDefaultAsync(e => e.EstadoTareaId == estadoTareaId, cancellationToken);

            return estado is null
                ? ResultadoOperacion<EstadoTareaDto>.NoExiste("El estado de tarea no existe.")
                : ResultadoOperacion<EstadoTareaDto>.Exitoso(Mapear(estado, 0));
        }

        public async Task<ResultadoOperacion<EstadoTareaDto>> CrearAsync(int tableroId, CrearEstadoTareaDto dto, CancellationToken cancellationToken = default)
        {
            if (!await _context.Tableros.AnyAsync(t => t.TableroId == tableroId, cancellationToken))
            {
                return ResultadoOperacion<EstadoTareaDto>.NoExiste("El tablero no existe.");
            }

            var nombre = dto.Nombre.Trim();

            // Regla de nombre único: no puede haber dos estados con el mismo nombre en el tablero.
            if (await _context.EstadoTareas.AnyAsync(e => e.TableroId == tableroId && e.Nombre == nombre, cancellationToken))
            {
                return ResultadoOperacion<EstadoTareaDto>.Fallo(nameof(dto.Nombre), "Ya existe un estado con ese nombre en el tablero.");
            }

            var estado = new EstadoTarea { TableroId = tableroId, Nombre = nombre, Descripcion = dto.Descripcion.Trim() };
            _context.EstadoTareas.Add(estado);
            _auditoria.Registrar(tableroId, $"Creó el estado \"{estado.Nombre}\".");
            await _context.SaveChangesAsync(cancellationToken);

            return ResultadoOperacion<EstadoTareaDto>.Exitoso(Mapear(estado, 0), "Estado creado correctamente.");
        }

        public async Task<ResultadoOperacion<EstadoTareaDto>> ActualizarAsync(int estadoTareaId, ActualizarEstadoTareaDto dto, CancellationToken cancellationToken = default)
        {
            var estado = await _context.EstadoTareas.FirstOrDefaultAsync(e => e.EstadoTareaId == estadoTareaId, cancellationToken);

            if (estado is null)
            {
                return ResultadoOperacion<EstadoTareaDto>.NoExiste("El estado de tarea no existe.");
            }

            // Regla de estados base inmutables: Kanban y el panel dependen de ellos.
            if (EstadosTareaSistema.Todos.Contains(estado.Nombre))
            {
                return ResultadoOperacion<EstadoTareaDto>.Fallo("Los estados base no se pueden modificar.");
            }

            var nombre = dto.Nombre.Trim();

            // Regla de nombre único: no puede haber dos estados con el mismo nombre en el tablero.
            if (await _context.EstadoTareas.AnyAsync(e => e.TableroId == estado.TableroId && e.Nombre == nombre && e.EstadoTareaId != estadoTareaId, cancellationToken))
            {
                return ResultadoOperacion<EstadoTareaDto>.Fallo(nameof(dto.Nombre), "Ya existe un estado con ese nombre en el tablero.");
            }

            estado.Nombre = nombre;
            estado.Descripcion = dto.Descripcion.Trim();
            _auditoria.Registrar(estado.TableroId, $"Editó el estado \"{estado.Nombre}\".");
            await _context.SaveChangesAsync(cancellationToken);

            var cantidadTareas = await _context.Tareas.CountAsync(t => t.EstadoTareaId == estadoTareaId, cancellationToken);
            return ResultadoOperacion<EstadoTareaDto>.Exitoso(Mapear(estado, cantidadTareas), "Estado actualizado correctamente.");
        }

        public async Task<ResultadoOperacion> EliminarAsync(int estadoTareaId, CancellationToken cancellationToken = default)
        {
            var estado = await _context.EstadoTareas.FirstOrDefaultAsync(e => e.EstadoTareaId == estadoTareaId, cancellationToken);

            if (estado is null)
            {
                return ResultadoOperacion.NoExiste("El estado de tarea no existe.");
            }

            // Regla de estados base inmutables: Kanban y el panel dependen de ellos.
            if (EstadosTareaSistema.Todos.Contains(estado.Nombre))
            {
                return ResultadoOperacion.Fallo("Los estados base no se pueden eliminar.");
            }

            // No eliminación de estados en listas: no puede estar vinculado a una lista.
            if (await _context.ListaTareas.AnyAsync(l => l.EstadoTareaId == estadoTareaId, cancellationToken))
            {
                return ResultadoOperacion.Fallo("No se puede eliminar un estado vinculado a una lista.");
            }

            // No eliminación de estados en uso: no puede tener tareas asociadas.
            if (await _context.Tareas.AnyAsync(t => t.EstadoTareaId == estadoTareaId, cancellationToken))
            {
                return ResultadoOperacion.Fallo("No se puede eliminar un estado que tiene tareas asociadas.");
            }

            _context.EstadoTareas.Remove(estado);
            _auditoria.Registrar(estado.TableroId, $"Eliminó el estado \"{estado.Nombre}\".");
            await _context.SaveChangesAsync(cancellationToken);

            return ResultadoOperacion.Exitoso("Estado eliminado correctamente.");
        }

        private static EstadoTareaDto Mapear(EstadoTarea estado, int cantidadTareas) => new()
        {
            EstadoTareaId = estado.EstadoTareaId,
            Nombre = estado.Nombre,
            Descripcion = estado.Descripcion,
            CantidadTareas = cantidadTareas,
            EsBase = EstadosTareaSistema.Todos.Contains(estado.Nombre)
        };
    }
}
