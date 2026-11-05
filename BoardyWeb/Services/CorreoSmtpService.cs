using System.Net;
using System.Net.Mail;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace BoardyWeb.Services
{
    /// <summary>
    /// Envío de correos por SMTP. Identity lo usa para confirmación y recuperación de cuenta.
    /// </summary>
    public class CorreoSmtpService : IEmailSender
    {
        private readonly IConfiguration _configuracion;

        public CorreoSmtpService(IConfiguration configuracion)
        {
            _configuracion = configuracion;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var smtp = _configuracion.GetSection("Smtp");
            var servidor = smtp["Servidor"];

            if (string.IsNullOrWhiteSpace(servidor))
            {
                throw new InvalidOperationException("Falta configurar la sección Smtp.");
            }

            using var cliente = new SmtpClient(servidor, smtp.GetValue<int>("Puerto"))
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(smtp["Usuario"], smtp["Clave"])
            };

            using var mensaje = new MailMessage(smtp["Remitente"]!, email, subject, htmlMessage) { IsBodyHtml = true };

            await cliente.SendMailAsync(mensaje);
        }
    }
}
