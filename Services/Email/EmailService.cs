using BaseConLogin.Data;
using BaseConLogin.Models;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Mail;

namespace BaseConLogin.Services.Email
{
    public class EmailService : IEmailService
    {
        private readonly ApplicationDbContext _context;

        // UN SOLO CONSTRUCTOR: El sistema ahora sabe exactamente qué inyectar
        public EmailService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var config = await _context.ConfiguracionEmails.FirstOrDefaultAsync();

            if (config == null) throw new Exception("Configuración SMTP no encontrada en la base de datos.");

            using (var client = new SmtpClient(config.ServidorSmtp, config.Puerto))
            {
                client.Credentials = new NetworkCredential(config.Usuario, config.Password);
                client.EnableSsl = config.UsarSsl;
                client.Timeout = 10000; // 10 segundos para evitar que se cuelgue

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(config.Usuario, config.RemitenteNombre),
                    Subject = subject,
                    Body = ObtenerPlantillaMaster(htmlMessage, config),
                    IsBodyHtml = true
                };
                mailMessage.To.Add(email);

                await client.SendMailAsync(mailMessage);
            }
        }

        private string ObtenerPlantillaMaster(string contenidoCuerpo, ConfiguracionEmail config)
        {
            string nombreTienda = config.RemitenteNombre ?? "Nuestra Tienda";

            return $@"
            <html>
            <head>
                <style>
                    .btn-link:hover {{ text-decoration: underline !important; }}
                </style>
            </head>
            <body style='margin:0; padding:0; background-color:#f4f7f9; font-family:""Segoe UI"", Tahoma, Geneva, Verdana, sans-serif;'>
                <table width='100%' border='0' cellspacing='0' cellpadding='0' style='background-color:#f4f7f9; padding: 20px 0;'>
                    <tr>
                        <td align='center'>
                            <table width='600' border='0' cellspacing='0' cellpadding='0' style='background-color:#ffffff; border-radius:12px; overflow:hidden; box-shadow: 0 4px 10px rgba(0,0,0,0.05);'>
                                <tr>
                                    <td align='center' style='padding: 35px 0; background: linear-gradient(135deg, #1a1a1a 0%, #333333 100%);'>
                                        <h1 style='color:#ffffff; margin:0; font-size:26px; letter-spacing: 2px; text-transform: uppercase;'>
                                            {nombreTienda}
                                        </h1>
                                    </td>
                                </tr>
                                <tr>
                                    <td style='padding: 40px; color:#444444; line-height:1.6; font-size:16px;'>
                                        {contenidoCuerpo}
                                    </td>
                                </tr>
                                <tr>
                                    <td align='center' style='padding: 30px; background-color:#fdfdfd; border-top:1px solid #eeeeee;'>
                                        <p style='margin:0; color:#999999; font-size:12px;'>
                                            &copy; {DateTime.Now.Year} <strong>{nombreTienda}</strong>
                                        </p>
                                        <p style='margin:5px 0; color:#bbbbbb; font-size:11px;'>
                                            Enviado desde nuestra sede usando: {config.ServidorSmtp}
                                        </p>
                                        <div style='margin-top:20px; padding-top:20px; border-top: 1px solid #f0f0f0;'>
                                            <p style='margin:0; font-size:10px; color:#cccccc; line-height: 1.4;'>
                                                Recibes este correo porque estás suscrito a nuestra newsletter.<br>
                                                Si deseas dejar de recibir estos correos, puedes 
                                                <a href='#' style='color:#0d6efd; text-decoration:none;'>darte de baja aquí</a>.
                                            </p>
                                        </div>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                </table>
            </body>
            </html>";
        }

        public async Task SendNewsletterAsync(List<string> emails, string subject, string htmlMessage)
        {
            foreach (var email in emails)
            {
                try
                {
                    await SendEmailAsync(email, subject, htmlMessage);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error enviando a {email}: {ex.Message}");
                }
                await Task.Delay(150);
            }
        }
    }
}