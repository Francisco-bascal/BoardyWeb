using BoardyWeb.Comun;
using BoardyWeb.Data;
using BoardyWeb.DTOs;
using BoardyWeb.Models;
using Microsoft.EntityFrameworkCore;

namespace BoardyWeb.Services
{
    public class AuditoriaService : IAuditoriaService
    {
        private readonly AppDbContext _context;

        public AuditoriaService(AppDbContext context)
        {
            _context = context;
        }

        public async Task RegistrarAsync(int tableroId, string usuarioId, string accion, CancellationToken cancellationToken = default)
        {
            _context.RegistrosAuditoria.Add(new RegistroAuditoria
            {
                TableroId = tableroId,
                UsuarioId = usuarioId,
                Accion = accion,
                Fecha = DateTime.UtcNow
            });

            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<Pagina<RegistroAuditoriaDto>> ListarAsync(int tableroId, string? usuarioId, int pagina, CancellationToken cancellationToken = default)
        {
            var numero = Math.Max(1, pagina);
            var registros = _context.RegistrosAuditoria
                .AsNoTracking()
                .Where(r => r.TableroId == tableroId && (usuarioId == null || r.UsuarioId == usuarioId));

            return new Pagina<RegistroAuditoriaDto>
            {
                Numero = numero,
                TotalElementos = await registros.CountAsync(cancellationToken),
                Elementos = await registros
                    .OrderByDescending(r => r.Fecha)
                    .Skip((numero - 1) * Pagina<RegistroAuditoriaDto>.Tamanio)
                    .Take(Pagina<RegistroAuditoriaDto>.Tamanio)
                    .Select(r => new RegistroAuditoriaDto
                    {
                        UsuarioNombre = r.Usuario.Nombre + " " + r.Usuario.Apellido,
                        Accion = r.Accion,
                        Fecha = r.Fecha
                    })
                    .ToListAsync(cancellationToken)
            };
        }
    }
}
