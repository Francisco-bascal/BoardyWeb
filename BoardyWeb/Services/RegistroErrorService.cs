using BoardyWeb.Comun;
using BoardyWeb.Data;
using BoardyWeb.DTOs;
using BoardyWeb.Models;
using Microsoft.EntityFrameworkCore;

namespace BoardyWeb.Services
{
    /// <summary>
    /// Guarda y lista los errores no controlados de la aplicación.
    /// </summary>
    public class RegistroErrorService : IRegistroErrorService
    {
        private readonly AppDbContext _context;

        public RegistroErrorService(AppDbContext context)
        {
            _context = context;
        }

        // Guarda un error con su detalle, ruta y usuario.
        public async Task RegistrarAsync(string mensaje, string detalle, string ruta, string? usuarioId, CancellationToken cancellationToken = default)
        {
            _context.RegistrosError.Add(new RegistroError
            {
                Mensaje = mensaje,
                Detalle = detalle,
                Ruta = ruta,
                UsuarioId = usuarioId,
                Fecha = DateTime.UtcNow
            });

            await _context.SaveChangesAsync(cancellationToken);
        }

        // Lista paginada para Plataforma → Logs.
        public async Task<Pagina<RegistroErrorDto>> ListarAsync(int pagina, CancellationToken cancellationToken = default)
        {
            var numero = Math.Max(1, pagina);

            return new Pagina<RegistroErrorDto>
            {
                Numero = numero,
                TotalElementos = await _context.RegistrosError.CountAsync(cancellationToken),
                Elementos = await _context.RegistrosError
                    .AsNoTracking()
                    .OrderByDescending(r => r.Fecha)
                    .Skip((numero - 1) * Pagina<RegistroErrorDto>.Tamanio)
                    .Take(Pagina<RegistroErrorDto>.Tamanio)
                    .Select(r => new RegistroErrorDto
                    {
                        Mensaje = r.Mensaje,
                        Detalle = r.Detalle,
                        Ruta = r.Ruta,
                        UsuarioId = r.UsuarioId,
                        Fecha = r.Fecha
                    })
                    .ToListAsync(cancellationToken)
            };
        }
    }
}
