#nullable disable

using System.Text;
using System.Threading.Tasks;
using BoardyWeb.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace BoardyWeb.Areas.Identity.Pages.Account
{
    public class RegisterConfirmationModel : PageModel
    {
        private readonly UserManager<Usuario> _userManager;

        public RegisterConfirmationModel(UserManager<Usuario> userManager)
        {
            _userManager = userManager;
        }

        public string Email { get; set; }

        public bool DisplayConfirmAccountLink { get; set; }

        public string EmailConfirmationUrl { get; set; }

        public async Task<IActionResult> OnGetAsync(string email, string returnUrl = null)
        {
            if (email == null)
            {
                return RedirectToPage("/Index");
            }

            returnUrl ??= Url.Content("~/");

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return NotFound($"No se pudo cargar el usuario con el correo electrónico '{email}'.");
            }

            Email = email;

            // Mientras no haya un emisor de correo real configurado, el enlace de confirmación
            // se muestra en pantalla para poder completar el registro. Al configurar un
            // IEmailSender real, quitar este código y el enlace de la vista.
            DisplayConfirmAccountLink = true;
            if (DisplayConfirmAccountLink)
            {
                var userId = await _userManager.GetUserIdAsync(user);
                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                EmailConfirmationUrl = Url.Page(
                    "/Account/ConfirmEmail",
                    pageHandler: null,
                    values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                    protocol: Request.Scheme);
            }

            return Page();
        }
    }
}
