using BoardyWeb.Comun;
using BoardyWeb.Data;
using BoardyWeb.DTOs;
using BoardyWeb.Models;
using Microsoft.EntityFrameworkCore;

namespace BoardyWeb.Services
{
    public class ComentarioService : IComentarioService
    {
        private readonly AppDbContext _context;
        private readonly IAuditoriaService _auditoria;

        public ComentarioService(AppDbContext context, IAuditoriaService auditoria)
        {
            _context = context;
            _auditoria = auditoria;
        }

        public async Task<IReadOnlyList<ComentarioDto>> ListarPorTareaAsync(int tareaId, CancellationToken cancellationToken = default)
        {
            return await _context.Comentarios
                .AsNoTracking()
                .Where(c => c.TareaId == tareaId)
                .OrderBy(c => c.FechaCreacion)
                .Select(c => new ComentarioDto
                {
                    ComentarioId = c.ComentarioId,
                    Contenido = c.Contenido,
                    FechaCreacion = c.FechaCreacion,
                    UsuarioId = c.UsuarioId,
                    UsuarioNombre = c.Usuario.Nombre + " " + c.Usuario.Apellido
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<int?> ObtenerTableroIdAsync(int comentarioId, CancellationToken cancellationToken = default)
        {
            return await _context.Comentarios
                .AsNoTracking()
                .Where(c => c.ComentarioId == comentarioId)
                .Select(c => (int?)c.Tarea.ListaTarea.TableroId)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<ResultadoOperacion<ComentarioDto>> CrearAsync(int tareaId, CrearComentarioDto dto, string usuarioId, CancellationToken cancellationToken = default)
        {
            if (!await _context.Tareas.AnyAsync(t => t.TareaId == tareaId, cancellationToken))
            {
                return ResultadoOperacion<ComentarioDto>.NoExiste("La tarea no existe.");
            }

            var comentario = new Comentario
            {
                TareaId = tareaId,
                UsuarioId = usuarioId,
                Contenido = dto.Contenido.Trim()
            };

            _context.Comentarios.Add(comentario);
            var tarea = await _context.Tareas.Where(t => t.TareaId == tareaId).Select(t => new { t.Titulo, t.ListaTarea.TableroId }).FirstAsync(cancellationToken);
            _auditoria.Registrar(tarea.TableroId, $"Comentó en la tarea \"{tarea.Titulo}\".");
            await _context.SaveChangesAsync(cancellationToken);

            var resultado = await _context.Comentarios
                .AsNoTracking()
                .Where(c => c.ComentarioId == comentario.ComentarioId)
                .Select(c => new ComentarioDto
                {
                    ComentarioId = c.ComentarioId,
                    Contenido = c.Contenido,
                    FechaCreacion = c.FechaCreacion,
                    UsuarioId = c.UsuarioId,
                    UsuarioNombre = c.Usuario.Nombre + " " + c.Usuario.Apellido
                })
                .FirstAsync(cancellationToken);

            return ResultadoOperacion<ComentarioDto>.Exitoso(resultado, "Comentario agregado correctamente.");
        }

        public async Task<ResultadoOperacion> EliminarAsync(int comentarioId, string usuarioId, bool puedeModerar, CancellationToken cancellationToken = default)
        {
            var comentario = await _context.Comentarios.FirstOrDefaultAsync(c => c.ComentarioId == comentarioId, cancellationToken);

            if (comentario is null)
            {
                return ResultadoOperacion.NoExiste("El comentario no existe.");
            }

            // Regla de autoría: solo elimina el autor o quien puede moderar el tablero.
            if (comentario.UsuarioId != usuarioId && !puedeModerar)
            {
                return ResultadoOperacion.Prohibido("Solo el autor puede eliminar el comentario.");
            }

            _context.Comentarios.Remove(comentario);
            var tarea = await _context.Tareas.Where(t => t.TareaId == comentario.TareaId).Select(t => new { t.Titulo, t.ListaTarea.TableroId }).FirstAsync(cancellationToken);
            _auditoria.Registrar(tarea.TableroId, $"Eliminó un comentario de la tarea \"{tarea.Titulo}\".");
            await _context.SaveChangesAsync(cancellationToken);

            return ResultadoOperacion.Exitoso("Comentario eliminado correctamente.");
        }
    }
}
