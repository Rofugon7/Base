using System.ComponentModel.DataAnnotations;
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
    public class RegisterModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ILogger<RegisterModel> _logger;
        private readonly ITiendaContext _tiendaContext;
        private readonly ICarritoService _carritoService;

        public RegisterModel(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            ILogger<RegisterModel> logger,
            ITiendaContext tiendaContext,
            ICarritoService carritoService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _tiendaContext = tiendaContext;
            _carritoService = carritoService;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public string? ReturnUrl { get; set; }
        public int? TiendaId { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            [Display(Name = "Correo electrónico")]
            public string Email { get; set; } = string.Empty;

            [Required]
            [DataType(DataType.Password)]
            [Display(Name = "Contraseña")]
            public string Password { get; set; } = string.Empty;

            [DataType(DataType.Password)]
            [Display(Name = "Confirmar contraseña")]
            [Compare("Password", ErrorMessage = "La contraseña y la confirmación no coinciden.")]
            public string ConfirmPassword { get; set; } = string.Empty;
        }

        public void OnGet(string? returnUrl = null)
        {
            ReturnUrl = returnUrl;
            TiendaId = _tiendaContext.ObtenerTiendaIdOpcional();
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            TiendaId = _tiendaContext.ObtenerTiendaIdOpcional();

            if (!ModelState.IsValid)
                return Page();

            var user = new IdentityUser { UserName = Input.Email, Email = Input.Email };
            var result = await _userManager.CreateAsync(user, Input.Password);

            if (result.Succeeded)
            {
                _logger.LogInformation("Usuario creado correctamente.");

                // Loguear automáticamente
                await _signInManager.SignInAsync(user, isPersistent: false);

                // 🔹 Fusionar carrito de sesión con persistente si hay tienda
                if (TiendaId.HasValue)
                {
                    await _carritoService.FusionarCarritoSesionAsync(TiendaId.Value);
                }

                return LocalRedirect(returnUrl);
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return Page();
        }
    }
}
