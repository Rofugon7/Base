using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BaseConLogin.Data;
using BaseConLogin.Models;
using Microsoft.EntityFrameworkCore;

namespace BaseConLogin.Controllers.Admin
{
    // PRUEBA: Comenta temporalmente el [Authorize] para ver si es un problema de permisos
    // [Authorize(Roles = "Admin")] 
    public class ConfiguracionController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ConfiguracionController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Buscamos el registro 1 o creamos uno nuevo para la vista
            var config = await _context.TiendaConfigs.FirstOrDefaultAsync() ?? new TiendaConfig();
            return View(config);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(TiendaConfig model, IFormFile? LogoFile)
        {
            if (ModelState.IsValid)
            {
                if (LogoFile != null && LogoFile.Length > 0)
                {
                    // 1. Definir la ruta de la carpeta img
                    string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img");

                    // 2. SOLUCIÓN AL ERROR: Crear la carpeta si no existe
                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                    }

                    // 3. Generar el nombre del archivo (mantenemos la extensión original)
                    string extension = Path.GetExtension(LogoFile.FileName);
                    string fileName = "logo_tienda" + extension;
                    string fullPath = Path.Combine(folderPath, fileName);

                    // 4. Guardar el archivo físicamente
                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        await LogoFile.CopyToAsync(stream);
                    }

                    // 5. Guardar la ruta relativa en la base de datos
                    model.LogoPath = "/img/" + fileName;
                }
                var configExistente = await _context.TiendaConfigs.FirstOrDefaultAsync();

                if (configExistente == null)
                {
                    _context.Add(model);
                }
                else
                {
                    // Mapeo manual de campos para asegurar que se guardan todos
                    // Datos Básicos y Legales
                    configExistente.NombreEmpresa = model.NombreEmpresa;
                    configExistente.NombreComercial = model.NombreComercial;
                    configExistente.CIF = model.CIF;
                    configExistente.Direccion = model.Direccion;
                    configExistente.Telefono = model.Telefono;
                    configExistente.EmailContacto = model.EmailContacto;
                    configExistente.EmailAdministrador = model.EmailAdministrador;
                    configExistente.WebURL = model.WebURL;
                    configExistente.Horario = model.Horario;

                    // Facturación y Branding
                    configExistente.PrefijoFactura = model.PrefijoFactura;
                    configExistente.IvaPorDefecto = model.IvaPorDefecto;
                    configExistente.LogoPath = model.LogoPath;
                    configExistente.ColorCorporativo = model.ColorCorporativo;
                    configExistente.ColorBotones = model.ColorBotones;
                    configExistente.ColorBotonesTexto = model.ColorBotonesTexto;
                    configExistente.ColorEnlaces  = model.ColorEnlaces;
                    configExistente.ColorTextos = model.ColorTextos;


                    // NUEVOS CAMPOS: Registro Mercantil
                    configExistente.RegistroMercantil = model.RegistroMercantil;
                    configExistente.Tomo = model.Tomo;
                    configExistente.Libro = model.Libro;
                    configExistente.Folio = model.Folio;
                    configExistente.Inscripcion = model.Inscripcion;
                    configExistente.Actividad = model.Actividad;
                    configExistente.Representante = model.Representante;

                    // NUEVOS CAMPOS: RRSS y Maps
                    configExistente.Maps = model.Maps;
                    configExistente.UrlInstagram = model.UrlInstagram;
                    configExistente.UrlFacebook = model.UrlFacebook;
                    configExistente.UrlX = model.UrlX;
                    configExistente.UrlTikTok = model.UrlTikTok;
                    configExistente.Whatsapp = model.Whatsapp;
                    configExistente.MapaIncrustado = model.MapaIncrustado;

                    configExistente.MaxFileSizeMB = model.MaxFileSizeMB;
                    configExistente.FormatosPermitidos = model.FormatosPermitidos;
                    configExistente.PermitirRecogidaTienda = model.PermitirRecogidaTienda;
                    configExistente.EnvioEstandar = model.EnvioEstandar;
                    configExistente.EnvioUrgente = model.EnvioUrgente;
                    configExistente.EnvioGratisDesde = model.EnvioGratisDesde;

                    configExistente.IbanTransferencia = model.IbanTransferencia;
                    configExistente.TitularCuenta = model.TitularCuenta;
                    configExistente.NombreBanco = model.NombreBanco;

                    _context.Update(configExistente);
                }

                await _context.SaveChangesAsync();
                TempData["Success"] = "Configuración guardada correctamente.";
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }
    }
}