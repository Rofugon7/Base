using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Threading.Tasks;
using BaseConLogin.Services.Carritos;
using BaseConLogin.Services.Tiendas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace BaseConLogin.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ILogger<LoginModel> _logger;
        private readonly ITiendaContext _tiendaContext;
        private readonly ICarritoService _carritoService;
        private readonly UserManager<IdentityUser> _userManager;

        public LoginModel(
            SignInManager<IdentityUser> signInManager,
            UserManager<IdentityUser> userManager,
            ILogger<LoginModel> logger,
            ITiendaContext tiendaContext,
            ICarritoService carritoService)
        {
            _signInManager = signInManager;
            _logger = logger;
            _tiendaContext = tiendaContext;
            _carritoService = carritoService;
            _userManager = userManager;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public string? ReturnUrl { get; set; }
        public int? TiendaId { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; } = string.Empty;

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; } = string.Empty;

            [Display(Name = "Recordarme")]
            public bool RememberMe { get; set; }
        }

        public void OnGet(string? returnUrl = null)
        {
            ReturnUrl = returnUrl;
            TiendaId = _tiendaContext.ObtenerTiendaIdOpcional();
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            if (!ModelState.IsValid)
                return Page();

            var result = await _signInManager.PasswordSignInAsync(
                Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                _logger.LogInformation("Usuario logueado.");

                // 🔹 Obtener usuario
                var user = await _userManager.FindByEmailAsync(Input.Email);

                if (user != null)
                {
                    // 🔹 Obtener tienda asociada (la primera activa)
                    var tiendaId = await _tiendaContext.ObtenerTiendaIdUsuarioAsync(user.Id);

                    if (tiendaId.HasValue)
                    {
                        // 🔹 Agregar claim al usuario si no existe
                        var existingClaim = (await _userManager.GetClaimsAsync(user))
                            .FirstOrDefault(c => c.Type == "TiendaId");

                        if (existingClaim != null)
                        {
                            await _userManager.RemoveClaimAsync(user, existingClaim);
                        }

                        await _userManager.AddClaimAsync(user, new Claim("TiendaId", tiendaId.Value.ToString()));

                        // 🔹 Fusionar carrito de sesión con persistente
                        await _carritoService.FusionarCarritoSesionAsync(tiendaId.Value);
                    }
                }

                return LocalRedirect(returnUrl);
            }

            if (result.RequiresTwoFactor)
            {
                return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
            }
            if (result.IsLockedOut)
            {
                _logger.LogWarning("Usuario bloqueado.");
                return RedirectToPage("./Lockout");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Intento de inicio de sesión inválido.");
                return Page();
            }
        }
    }
}
