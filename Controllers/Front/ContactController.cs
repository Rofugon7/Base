using BaseConLogin.Data;
using BaseConLogin.Models;
using BaseConLogin.Services.Seo;
using Microsoft.AspNetCore.Mvc;

namespace BaseConLogin.Controllers.Front
{
    [Route("contacto")]
    public class ContactController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ICanonicalService _canonical;

        public ContactController(
            ApplicationDbContext context,
            ICanonicalService canonical)
        {
            _context = context;
            _canonical = canonical;
        }

        // =========================
        // GET: /contacto
        // =========================
        [HttpGet("")]
        public IActionResult Index()
        {
            // === CANONICAL ===
            ViewData["Canonical"] = _canonical.Build("contacto");

            // === SEO ===
            ViewData["Title"] =
                "Contacto | Atención al cliente y soporte";

            ViewData["Description"] =
                "Contacta con nuestro equipo para resolver dudas, solicitar información o recibir soporte personalizado.";

            return View();
        }

        // =========================
        // POST: /contacto
        // =========================
        [HttpPost("")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(ContactMessage contactMessage)
        {
            if (!ModelState.IsValid)
            {
                // Mantener SEO incluso con error
                ViewData["Canonical"] = _canonical.Build("contacto");

                ViewData["Title"] =
                    "Contacto | Atención al cliente y soporte";

                ViewData["Description"] =
                    "Contacta con nuestro equipo para resolver dudas, solicitar información o recibir soporte personalizado.";

                return View(contactMessage);
            }

            _context.Add(contactMessage);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "✅ ¡Gracias! Hemos recibido tu mensaje y te contactaremos pronto.";

            // Redirección PRG (Post-Redirect-Get) => evita duplicados
            return RedirectToAction(nameof(Index));
        }
    }
}
