using System.Linq.Expressions;
using System.Security.Claims;
using BoardyWeb.Comun;
using BoardyWeb.Data;
using BoardyWeb.DTOs;
using BoardyWeb.Models;
using Microsoft.EntityFrameworkCore;

namespace BoardyWeb.Services
{
    /// <summary>
    /// Registra y consulta las acciones realizadas en cada tablero.
    /// </summary>
    public class AuditoriaService : IAuditoriaService
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuditoriaService(AppDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        // Agrega la acción sin guardar: se persiste con el SaveChanges de la operación.
        public void Registrar(int tableroId, string accion)
        {
            var usuarioId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (usuarioId is null)
            {
                return;
            }

            _context.RegistrosAuditoria.Add(new RegistroAuditoria
            {
                TableroId = tableroId,
                UsuarioId = usuarioId,
                Accion = accion,
                Fecha = DateTime.UtcNow
            });
        }

        // Lista paginada para la pantalla de auditoría.
        public async Task<Pagina<RegistroAuditoriaDto>> ListarAsync(int tableroId, string? usuarioId, int pagina, CancellationToken cancellationToken = default)
        {
            var numero = Math.Max(1, pagina);
            var registros = Filtrar(tableroId, usuarioId);

            return new Pagina<RegistroAuditoriaDto>
            {
                Numero = numero,
                TotalElementos = await registros.CountAsync(cancellationToken),
                Elementos = await registros
                    .OrderByDescending(r => r.Fecha)
                    .Skip((numero - 1) * Pagina<RegistroAuditoriaDto>.Tamanio)
                    .Take(Pagina<RegistroAuditoriaDto>.Tamanio)
                    .Select(Proyectar)
                    .ToListAsync(cancellationToken)
            };
        }

        // Lista completa para el PDF.
        public async Task<IReadOnlyList<RegistroAuditoriaDto>> ListarTodoAsync(int tableroId, string? usuarioId, CancellationToken cancellationToken = default)
        {
            return await Filtrar(tableroId, usuarioId)
                .OrderByDescending(r => r.Fecha)
                .Select(Proyectar)
                .ToListAsync(cancellationToken);
        }

        // Filtra por tablero y, si se indicó, por usuario.
        private IQueryable<RegistroAuditoria> Filtrar(int tableroId, string? usuarioId)
        {
            return _context.RegistrosAuditoria
                .AsNoTracking()
                .Where(r => r.TableroId == tableroId && (string.IsNullOrEmpty(usuarioId) || r.UsuarioId == usuarioId));
        }

        private static readonly Expression<Func<RegistroAuditoria, RegistroAuditoriaDto>> Proyectar = r => new RegistroAuditoriaDto
        {
            UsuarioNombre = r.Usuario.Nombre + " " + r.Usuario.Apellido,
            Accion = r.Accion,
            Fecha = r.Fecha
        };
    }
}
