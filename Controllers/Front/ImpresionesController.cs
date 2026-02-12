using BaseConLogin.Data;
using BaseConLogin.Models;
using BaseConLogin.Services;
using BaseConLogin.Services.Tiendas;
using BaseConLogin.Services.TrabajosImpresion;
using BaseConLogin.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;


[Authorize]
public class ImpresionesController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IImpresionService _impresionService;
    private readonly ITiendaConfigService _configService;

    public ImpresionesController(
        ApplicationDbContext context,
        IImpresionService impresionService,
        ITiendaConfigService configService)
    {
        _context = context;
        _impresionService = impresionService;
        _configService = configService;
    }

    // --- VISTAS ---

    public async Task<IActionResult> MisImpresiones()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var trabajos = await _context.TrabajosImpresion
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.FechaSubida)
            .ToListAsync();
        return View(trabajos);
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> PanelAdmin(string searchNif)
    {
        var query = _context.TrabajosImpresion
                        .Include(t => t.Usuario)
                        .AsQueryable();

        if (!string.IsNullOrEmpty(searchNif))
        {
            var nifBusqueda = _impresionService.LimpiarNif(searchNif);
            query = query.Where(t => t.NifLimpio.Contains(nifBusqueda));
        }

        return View(await query.OrderByDescending(t => t.FechaSubida).ToListAsync());
    }

    public async Task<IActionResult> Subir()
    {
        var config = await _configService.GetConfigAsync();
        ViewBag.Formatos = config.FormatosPermitidos;
        ViewBag.MaxSize = config.MaxFileSizeMB;
        return View();
    }

    // --- ACCIONES DE ARCHIVO ---

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Subir(SubirArchivoViewModel model, bool confirmOverwrite)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == User.FindFirstValue(ClaimTypes.NameIdentifier));
        var config = await _configService.GetConfigAsync();

        // 1. Validaciones Técnicas
        var extension = Path.GetExtension(model.Archivo.FileName).ToLower();

        bool formatoValido = config.FormatosPermitidos
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(f => f.Trim().ToLower())
            .Contains(extension);

        if (!formatoValido)
        {
            ModelState.AddModelError("Archivo", "Formato no permitido.");
            return View(model);
        }

        if (model.Archivo.Length > (config.MaxFileSizeMB * 1024 * 1024))
        {
            ModelState.AddModelError("Archivo", $"El archivo supera los {config.MaxFileSizeMB}MB.");
            return View(model);
        }

        var nifLimpio = _impresionService.LimpiarNif(user.NifCif);
        var rutaCarpeta = _impresionService.ObtenerRutaUsuario(nifLimpio);
        var rutaCompleta = Path.Combine(rutaCarpeta, model.Archivo.FileName);

        // 2. Lógica de Sobreescritura
        bool existe = await _impresionService.ArchivoExisteAsync(nifLimpio, model.Archivo.FileName);
        if (existe && !confirmOverwrite)
        {
            // Este caso no debería darse por el JS del modal, pero por seguridad:
            return BadRequest("El archivo ya existe y no se confirmó la sobreescritura.");
        }

        // 3. Guardado Físico
        await _impresionService.GuardarArchivoAsync(model.Archivo, rutaCompleta);

        // 4. Registro en BD (Solo si es nuevo o no queremos duplicar historial)
        var registroExistente = await _context.TrabajosImpresion
            .FirstOrDefaultAsync(t => t.NifLimpio == nifLimpio && t.NombreArchivoOriginal == model.Archivo.FileName && !t.ArchivoEliminado);

        if (registroExistente == null)
        {
            var nuevoTrabajo = new TrabajoImpresion
            {
                UserId = user.Id,
                Usuario = user, 
                NifLimpio = nifLimpio,
                NombreArchivoOriginal = model.Archivo.FileName,
                NombreArchivoServidor = model.Archivo.FileName, // Podrías usar un GUID aquí si prefieres
                RutaFisica = rutaCompleta,
                Notas = model.Notas,
                Estado = "Pendiente"
            };
            _context.TrabajosImpresion.Add(nuevoTrabajo);
        }
        else
        {
            registroExistente.FechaSubida = DateTime.Now;
            registroExistente.Estado = "Pendiente";
            registroExistente.Notas = model.Notas;
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(MisImpresiones));
    }

    [HttpGet]
    public async Task<IActionResult> ExisteArchivo(string nombre)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == User.FindFirstValue(ClaimTypes.NameIdentifier));
        var nifLimpio = _impresionService.LimpiarNif(user.NifCif);
        bool exists = await _impresionService.ArchivoExisteAsync(nifLimpio, nombre);
        return Json(new { exists });
    }

    public async Task<IActionResult> Descargar(int id)
    {
        var trabajo = await _context.TrabajosImpresion.FindAsync(id);
        if (trabajo == null || trabajo.ArchivoEliminado) return NotFound();

        if (!User.IsInRole("Admin") && trabajo.UserId != User.FindFirstValue(ClaimTypes.NameIdentifier))
            return Forbid();

        var stream = System.IO.File.OpenRead(trabajo.RutaFisica);
        return File(stream, "application/octet-stream", trabajo.NombreArchivoOriginal);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CambiarEstado(int id, string nuevoEstado)
    {
        var trabajo = await _context.TrabajosImpresion.FindAsync(id);
        if (trabajo != null)
        {
            trabajo.Estado = nuevoEstado;
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(PanelAdmin));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> EliminarArchivo(int id)
    {
        var trabajo = await _context.TrabajosImpresion.FindAsync(id);
        if (trabajo != null)
        {
            _impresionService.EliminarArchivoFisico(trabajo.RutaFisica);
            trabajo.ArchivoEliminado = true;
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(PanelAdmin));
    }
}