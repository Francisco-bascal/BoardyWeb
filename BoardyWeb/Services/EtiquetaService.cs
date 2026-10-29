using BoardyWeb.Comun;
using BoardyWeb.Data;
using BoardyWeb.DTOs;
using BoardyWeb.Models;
using Microsoft.EntityFrameworkCore;

namespace BoardyWeb.Services
{
    public class EtiquetaService : IEtiquetaService
    {
        private readonly AppDbContext _context;

        public EtiquetaService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<EtiquetaDto>> ListarAsync(int tableroId, CancellationToken cancellationToken = default)
        {
            return await _context.Etiquetas
                .AsNoTracking()
                .Where(e => e.TableroId == tableroId)
                .OrderBy(e => e.Nombre)
                .Select(e => new EtiquetaDto
                {
                    EtiquetaId = e.EtiquetaId,
                    Nombre = e.Nombre,
                    Descripcion = e.Descripcion,
                    Color = e.Color,
                    CantidadTareas = e.Tareas.Count
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<int?> ObtenerTableroIdAsync(int etiquetaId, CancellationToken cancellationToken = default)
        {
            return await _context.Etiquetas
                .AsNoTracking()
                .Where(e => e.EtiquetaId == etiquetaId)
                .Select(e => (int?)e.TableroId)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<ResultadoOperacion<EtiquetaDto>> CrearAsync(int tableroId, CrearEtiquetaDto dto, CancellationToken cancellationToken = default)
        {
            if (!await _context.Tableros.AnyAsync(t => t.TableroId == tableroId, cancellationToken))
            {
                return ResultadoOperacion<EtiquetaDto>.NoExiste("El tablero no existe.");
            }

            var nombre = dto.Nombre.Trim();

            if (await _context.Etiquetas.AnyAsync(e => e.TableroId == tableroId && e.Nombre == nombre, cancellationToken))
            {
                return ResultadoOperacion<EtiquetaDto>.Fallo(nameof(dto.Nombre), "Ya existe una etiqueta con ese nombre en el tablero.");
            }

            var etiqueta = new Etiqueta
            {
                TableroId = tableroId,
                Nombre = nombre,
                Descripcion = dto.Descripcion.Trim(),
                Color = dto.Color
            };

            _context.Etiquetas.Add(etiqueta);
            await _context.SaveChangesAsync(cancellationToken);

            return ResultadoOperacion<EtiquetaDto>.Exitoso(Mapear(etiqueta, 0), "Etiqueta creada correctamente.");
        }

        public async Task<ResultadoOperacion<EtiquetaDto>> ActualizarAsync(int etiquetaId, ActualizarEtiquetaDto dto, CancellationToken cancellationToken = default)
        {
            var etiqueta = await _context.Etiquetas.FirstOrDefaultAsync(e => e.EtiquetaId == etiquetaId, cancellationToken);

            if (etiqueta is null)
            {
                return ResultadoOperacion<EtiquetaDto>.NoExiste("La etiqueta no existe.");
            }

            var nombre = dto.Nombre.Trim();

            if (await _context.Etiquetas.AnyAsync(e => e.TableroId == etiqueta.TableroId && e.Nombre == nombre && e.EtiquetaId != etiquetaId, cancellationToken))
            {
                return ResultadoOperacion<EtiquetaDto>.Fallo(nameof(dto.Nombre), "Ya existe una etiqueta con ese nombre en el tablero.");
            }

            etiqueta.Nombre = nombre;
            etiqueta.Descripcion = dto.Descripcion.Trim();
            etiqueta.Color = dto.Color;
            await _context.SaveChangesAsync(cancellationToken);

            var cantidadTareas = await _context.TareaEtiquetas.CountAsync(te => te.EtiquetaId == etiquetaId, cancellationToken);
            return ResultadoOperacion<EtiquetaDto>.Exitoso(Mapear(etiqueta, cantidadTareas), "Etiqueta actualizada correctamente.");
        }

        public async Task<ResultadoOperacion> EliminarAsync(int etiquetaId, CancellationToken cancellationToken = default)
        {
            var etiqueta = await _context.Etiquetas.FirstOrDefaultAsync(e => e.EtiquetaId == etiquetaId, cancellationToken);

            if (etiqueta is null)
            {
                return ResultadoOperacion.NoExiste("La etiqueta no existe.");
            }

            if (await _context.TareaEtiquetas.AnyAsync(te => te.EtiquetaId == etiquetaId, cancellationToken))
            {
                return ResultadoOperacion.Fallo("No se puede eliminar una etiqueta que está asignada a tareas.");
            }

            _context.Etiquetas.Remove(etiqueta);
            await _context.SaveChangesAsync(cancellationToken);

            return ResultadoOperacion.Exitoso("Etiqueta eliminada correctamente.");
        }

        public async Task<ResultadoOperacion> AsignarATareaAsync(int tareaId, int etiquetaId, CancellationToken cancellationToken = default)
        {
            var tableroTareaId = await ObtenerTableroDeTareaAsync(tareaId, cancellationToken);
            if (tableroTareaId is null)
            {
                return ResultadoOperacion.NoExiste("La tarea no existe.");
            }

            var tableroEtiquetaId = await ObtenerTableroIdAsync(etiquetaId, cancellationToken);
            if (tableroEtiquetaId is null)
            {
                return ResultadoOperacion.NoExiste("La etiqueta no existe.");
            }

            if (tableroTareaId != tableroEtiquetaId)
            {
                return ResultadoOperacion.Fallo("La etiqueta no pertenece al tablero de la tarea.");
            }

            if (await _context.TareaEtiquetas.AnyAsync(te => te.TareaId == tareaId && te.EtiquetaId == etiquetaId, cancellationToken))
            {
                return ResultadoOperacion.Fallo("La tarea ya tiene esa etiqueta.");
            }

            _context.TareaEtiquetas.Add(new TareaEtiqueta { TareaId = tareaId, EtiquetaId = etiquetaId, FechaAsignacion = DateTime.UtcNow });
            await _context.SaveChangesAsync(cancellationToken);

            return ResultadoOperacion.Exitoso("Etiqueta asignada correctamente.");
        }

        public async Task<ResultadoOperacion> QuitarDeTareaAsync(int tareaId, int etiquetaId, CancellationToken cancellationToken = default)
        {
            var asignacion = await _context.TareaEtiquetas
                .FirstOrDefaultAsync(te => te.TareaId == tareaId && te.EtiquetaId == etiquetaId, cancellationToken);

            if (asignacion is null)
            {
                return ResultadoOperacion.NoExiste("La tarea no tiene esa etiqueta.");
            }

            _context.TareaEtiquetas.Remove(asignacion);
            await _context.SaveChangesAsync(cancellationToken);

            return ResultadoOperacion.Exitoso("Etiqueta quitada correctamente.");
        }

        private async Task<int?> ObtenerTableroDeTareaAsync(int tareaId, CancellationToken cancellationToken)
        {
            return await _context.Tareas
                .AsNoTracking()
                .Where(t => t.TareaId == tareaId)
                .Select(t => (int?)t.ListaTarea.TableroId)
                .FirstOrDefaultAsync(cancellationToken);
        }

        private static EtiquetaDto Mapear(Etiqueta etiqueta, int cantidadTareas) => new()
        {
            EtiquetaId = etiqueta.EtiquetaId,
            Nombre = etiqueta.Nombre,
            Descripcion = etiqueta.Descripcion,
            Color = etiqueta.Color,
            CantidadTareas = cantidadTareas
        };
    }
}
