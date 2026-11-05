using BoardyWeb.Comun;
using BoardyWeb.Models;
using Microsoft.AspNetCore.Identity;

namespace BoardyWeb.Services
{
    public class FotoPerfilService : IFotoPerfilService
    {
        private const long TamanioMaximo = 2 * 1024 * 1024;
        private const string Carpeta = "uploads";
        private const int BytesFirma = 12;

        private readonly UserManager<Usuario> _userManager;
        private readonly IWebHostEnvironment _entorno;

        public FotoPerfilService(UserManager<Usuario> userManager, IWebHostEnvironment entorno)
        {
            _userManager = userManager;
            _entorno = entorno;
        }

        public async Task<ResultadoOperacion> GuardarAsync(Usuario usuario, IFormFile archivo, CancellationToken cancellationToken = default)
        {
            if (archivo.Length == 0 || archivo.Length > TamanioMaximo)
            {
                return ResultadoOperacion.Fallo("La foto debe pesar como máximo 2 MB.");
            }

            await using var origen = archivo.OpenReadStream();
            var firma = new byte[BytesFirma];
            await origen.ReadAtLeastAsync(firma, BytesFirma, throwOnEndOfStream: false, cancellationToken);

            var extension = ObtenerExtension(firma);
            if (extension is null)
            {
                return ResultadoOperacion.Fallo("La foto debe ser JPG, PNG o WebP.");
            }

            var directorio = Path.Combine(_entorno.WebRootPath, Carpeta);
            Directory.CreateDirectory(directorio);

            var nombre = Guid.NewGuid().ToString("N") + extension;
            origen.Position = 0;
            await using (var destino = File.Create(Path.Combine(directorio, nombre)))
            {
                await origen.CopyToAsync(destino, cancellationToken);
            }

            return await ActualizarAsync(usuario, $"/{Carpeta}/{nombre}", "Foto de perfil actualizada.");
        }

        public Task<ResultadoOperacion> QuitarAsync(Usuario usuario)
        {
            return ActualizarAsync(usuario, null, "Foto de perfil eliminada.");
        }

        private async Task<ResultadoOperacion> ActualizarAsync(Usuario usuario, string? nuevaFoto, string mensaje)
        {
            var anterior = usuario.FotoPerfil;
            usuario.FotoPerfil = nuevaFoto;

            var resultado = await _userManager.UpdateAsync(usuario);
            if (!resultado.Succeeded)
            {
                return ResultadoOperacion.Fallo(string.Join(" ", resultado.Errors.Select(e => e.Description)));
            }

            if (anterior is not null)
            {
                File.Delete(Path.Combine(_entorno.WebRootPath, Carpeta, Path.GetFileName(anterior)));
            }

            return ResultadoOperacion.Exitoso(mensaje);
        }

        // Se valida el contenido real: la extensión y el Content-Type los controla el cliente.
        private static string? ObtenerExtension(byte[] firma)
        {
            if (firma.AsSpan(0, 3).SequenceEqual(new byte[] { 0xFF, 0xD8, 0xFF }))
            {
                return ".jpg";
            }

            if (firma.AsSpan(0, 8).SequenceEqual(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }))
            {
                return ".png";
            }

            var esRiff = firma.AsSpan(0, 4).SequenceEqual("RIFF"u8);
            var esWebp = firma.AsSpan(8, 4).SequenceEqual("WEBP"u8);

            return esRiff && esWebp ? ".webp" : null;
        }
    }
}
