using BaseConLogin.Data;
using BaseConLogin.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace BaseConLogin.Controllers
{
    public class ContactController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ContactController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(ContactMessage contactMessage)
        {
            if (ModelState.IsValid)
            {
                _context.Add(contactMessage);
                await _context.SaveChangesAsync();

                // Envío de correo configurado y comentado:
                /*
                var smtpClient = new SmtpClient("smtp.office365.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential("rofugon7@hotmail.com", "tu_contraseña"),
                    EnableSsl = true,
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress("rofugon7@hotmail.com"),
                    Subject = "Nuevo mensaje de contacto recibido",
                    Body = $"Nombre: {contactMessage.Name}\nTeléfono: {contactMessage.Phone}\nEmail: {contactMessage.Email}\nMensaje: {contactMessage.Message}",
                    IsBodyHtml = false,
                };
                mailMessage.To.Add("rofugon7@hotmail.com");

                await smtpClient.SendMailAsync(mailMessage);
                */

                TempData["SuccessMessage"] = "✅ ¡Gracias! Hemos recibido tu mensaje y te contactaremos pronto.";
                return RedirectToAction(nameof(Index));
            }
            return View(contactMessage);
        }
    }
}
