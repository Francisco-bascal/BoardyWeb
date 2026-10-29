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

        public ListaTareaService(AppDbContext context)
        {
            _context = context;
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
                    TableroId = l.TableroId
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

        public async Task<ResultadoOperacion<ListaTareaDto>> CrearAsync(int tableroId, CrearListaTareaDto dto, CancellationToken cancellationToken = default)
        {
            if (!await _context.Tableros.AnyAsync(t => t.TableroId == tableroId, cancellationToken))
            {
                return ResultadoOperacion<ListaTareaDto>.NoExiste("El tablero no existe.");
            }

            var ultimoOrden = await _context.ListaTareas
                .Where(l => l.TableroId == tableroId)
                .MaxAsync(l => (int?)l.Orden, cancellationToken) ?? 0;

            var lista = new ListaTarea
            {
                TableroId = tableroId,
                Nombre = dto.Nombre.Trim(),
                Descripcion = dto.Descripcion.Trim(),
                Orden = ultimoOrden + 1
            };

            _context.ListaTareas.Add(lista);
            await _context.SaveChangesAsync(cancellationToken);

            return ResultadoOperacion<ListaTareaDto>.Exitoso(Mapear(lista), "Lista creada correctamente.");
        }

        public async Task<ResultadoOperacion<ListaTareaDto>> ActualizarAsync(int listaTareaId, ActualizarListaTareaDto dto, CancellationToken cancellationToken = default)
        {
            var lista = await _context.ListaTareas.FirstOrDefaultAsync(l => l.ListaTareaId == listaTareaId, cancellationToken);

            if (lista is null)
            {
                return ResultadoOperacion<ListaTareaDto>.NoExiste("La lista no existe.");
            }

            lista.Nombre = dto.Nombre.Trim();
            lista.Descripcion = dto.Descripcion.Trim();
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

            if (await _context.Tareas.AnyAsync(t => t.ListaTareaId == listaTareaId, cancellationToken))
            {
                return ResultadoOperacion.Fallo("No se puede eliminar una lista que contiene tareas.");
            }

            _context.ListaTareas.Remove(lista);
            await _context.SaveChangesAsync(cancellationToken);

            return ResultadoOperacion.Exitoso("Lista eliminada correctamente.");
        }

        private static ListaTareaDto Mapear(ListaTarea lista) => new()
        {
            ListaTareaId = lista.ListaTareaId,
            Nombre = lista.Nombre,
            Descripcion = lista.Descripcion,
            Orden = lista.Orden,
            TableroId = lista.TableroId
        };
    }
}
