using BaseConLogin.Data;
using BaseConLogin.Models;
using BaseConLogin.Services.Email;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Mail;

namespace BaseConLogin.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
    public class NewsletterController : Controller
    {
        private readonly ApplicationDbContext _context;

        private readonly IEmailService _emailService;
        public NewsletterController(ApplicationDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        // Listado de suscriptores
        public async Task<IActionResult> Index()
        {
            var lista = await _context.Suscriptores
                .OrderByDescending(s => s.FechaSuscripcion)
                .ToListAsync();
            return View(lista);
        }

        // Acción para dar de baja (opcional)
        [HttpPost]
        public async Task<IActionResult> Eliminar(int id)
        {
            var suscriptor = await _context.Suscriptores.FindAsync(id);
            if (suscriptor != null)
            {
                _context.Suscriptores.Remove(suscriptor);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Enviar() => View();

        [HttpPost]
        public async Task<IActionResult> ProcesarEnvio(string asunto, string contenido)
        {
            var suscriptores = await _context.Suscriptores
                .Where(s => s.Activo)
                .Select(s => s.Email)
                .ToListAsync();

            await _emailService.SendNewsletterAsync(suscriptores, asunto, contenido);

            TempData["Success"] = $"Newsletter enviada con éxito a {suscriptores.Count} personas.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> EnviarPrueba(string email, string asunto, string contenido)
        {
            if (string.IsNullOrEmpty(email)) return BadRequest();

            try
            {
                // Usamos el servicio de email para enviar a una sola dirección
                await _emailService.SendEmailAsync(email, "[PRUEBA] " + asunto, contenido);
                return Ok();
            }
            catch
            {
                return StatusCode(500);
            }
        }

        public async Task<IActionResult> Configuracion()
        {
            var config = await _context.ConfiguracionEmails.FirstOrDefaultAsync() ?? new ConfiguracionEmail();
            return View(config);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GuardarConfiguracion(ConfiguracionEmail model)
        {
            if (ModelState.IsValid)
            {
                var configExistente = await _context.ConfiguracionEmails.FirstOrDefaultAsync();

                if (configExistente == null)
                {
                    _context.Add(model);
                }
                else
                {
                    // Actualizamos los campos manualmente para no perder el tracking
                    configExistente.ServidorSmtp = model.ServidorSmtp;
                    configExistente.Puerto = model.Puerto;
                    configExistente.Usuario = model.Usuario;
                    configExistente.Password = model.Password;
                    configExistente.RemitenteNombre = model.RemitenteNombre;
                    configExistente.UsarSsl = model.UsarSsl;
                    _context.Update(configExistente);
                }

                await _context.SaveChangesAsync();
                TempData["Success"] = "Configuración actualizada con éxito.";
                return RedirectToAction(nameof(Configuracion));
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ProbarConfiguracion(ConfiguracionEmail model)
        {
            // Intentamos enviar un correo de prueba a la misma dirección de usuario configurada
            // para verificar que las credenciales y el servidor son válidos.
            try
            {
                using (var client = new SmtpClient(model.ServidorSmtp, model.Puerto))
                {
                    client.Credentials = new NetworkCredential(model.Usuario, model.Password);
                    client.EnableSsl = model.UsarSsl;
                    client.Timeout = 10000; // 10 segundos de margen

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(model.Usuario, model.RemitenteNombre),
                        Subject = "Prueba de Conexión SMTP",
                        Body = "Si recibes este correo, tu configuración de Newsletter es correcta.",
                        IsBodyHtml = true
                    };
                    mailMessage.To.Add(model.Usuario);

                    await client.SendMailAsync(mailMessage);
                }
                return Ok(); // Todo bien
            }
            catch (Exception ex)
            {
                // Puedes loguear el error aquí para depuración: Console.WriteLine(ex.Message);
                return BadRequest("Error de conexión");
            }
        }
    }
}
