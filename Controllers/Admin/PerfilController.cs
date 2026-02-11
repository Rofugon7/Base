using BaseConLogin.Models;
using BaseConLogin.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BaseConLogin.Controllers.Admin
{
    [Authorize]
    public class PerfilController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public PerfilController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var model = new PerfilViewModel
            {
                NombreCompleto = user.NombreCompleto ?? "",
                Email = user.Email ?? "",
                Direccion = user.Direccion,
                Ciudad = user.Ciudad,
                DniCif = user.NifCif,
                CodigoPostal = user.CodigoPostal,
                TelefonoContacto = user.TelefonoContacto ?? user.PhoneNumber
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(PerfilViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            // Actualizamos los campos personalizados
            user.NombreCompleto = model.NombreCompleto;
            user.Direccion = model.Direccion;
            user.Ciudad = model.Ciudad;
            user.CodigoPostal = model.CodigoPostal;
            user.NifCif = model.DniCif;
            user.TelefonoContacto = model.TelefonoContacto;
            user.PhoneNumber = model.TelefonoContacto; // Sincronizamos con el campo base de Identity

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Perfil actualizado correctamente.";
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(model);
        }
    }
}