using BoardyWeb.Comun;
using BoardyWeb.Data;
using BoardyWeb.DTOs;
using BoardyWeb.Models;
using Microsoft.EntityFrameworkCore;

namespace BoardyWeb.Services
{
    public class EstadoTareaService : IEstadoTareaService
    {
        private readonly AppDbContext _context;

        public EstadoTareaService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<EstadoTareaDto>> ListarAsync(CancellationToken cancellationToken = default)
        {
            return await _context.EstadoTareas
                .AsNoTracking()
                .OrderBy(e => e.EstadoTareaId)
                .Select(e => new EstadoTareaDto
                {
                    EstadoTareaId = e.EstadoTareaId,
                    Nombre = e.Nombre,
                    Descripcion = e.Descripcion,
                    CantidadTareas = e.Tareas.Count
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<ResultadoOperacion<EstadoTareaDto>> ObtenerAsync(int estadoTareaId, CancellationToken cancellationToken = default)
        {
            var estado = await _context.EstadoTareas
                .AsNoTracking()
                .Where(e => e.EstadoTareaId == estadoTareaId)
                .Select(e => new EstadoTareaDto
                {
                    EstadoTareaId = e.EstadoTareaId,
                    Nombre = e.Nombre,
                    Descripcion = e.Descripcion,
                    CantidadTareas = e.Tareas.Count
                })
                .FirstOrDefaultAsync(cancellationToken);

            return estado is null
                ? ResultadoOperacion<EstadoTareaDto>.NoExiste("El estado de tarea no existe.")
                : ResultadoOperacion<EstadoTareaDto>.Exitoso(estado);
        }

        public async Task<ResultadoOperacion<EstadoTareaDto>> CrearAsync(CrearEstadoTareaDto dto, CancellationToken cancellationToken = default)
        {
            var nombre = dto.Nombre.Trim();

            if (await _context.EstadoTareas.AnyAsync(e => e.Nombre == nombre, cancellationToken))
            {
                return ResultadoOperacion<EstadoTareaDto>.Fallo(nameof(dto.Nombre), "Ya existe un estado con ese nombre.");
            }

            var estado = new EstadoTarea { Nombre = nombre, Descripcion = dto.Descripcion.Trim() };
            _context.EstadoTareas.Add(estado);
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

            var nombre = dto.Nombre.Trim();

            if (await _context.EstadoTareas.AnyAsync(e => e.Nombre == nombre && e.EstadoTareaId != estadoTareaId, cancellationToken))
            {
                return ResultadoOperacion<EstadoTareaDto>.Fallo(nameof(dto.Nombre), "Ya existe un estado con ese nombre.");
            }

            estado.Nombre = nombre;
            estado.Descripcion = dto.Descripcion.Trim();
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

            if (await _context.Tareas.AnyAsync(t => t.EstadoTareaId == estadoTareaId, cancellationToken))
            {
                return ResultadoOperacion.Fallo("No se puede eliminar un estado que tiene tareas asociadas.");
            }

            _context.EstadoTareas.Remove(estado);
            await _context.SaveChangesAsync(cancellationToken);

            return ResultadoOperacion.Exitoso("Estado eliminado correctamente.");
        }

        private static EstadoTareaDto Mapear(EstadoTarea estado, int cantidadTareas) => new()
        {
            EstadoTareaId = estado.EstadoTareaId,
            Nombre = estado.Nombre,
            Descripcion = estado.Descripcion,
            CantidadTareas = cantidadTareas
        };
    }
}
