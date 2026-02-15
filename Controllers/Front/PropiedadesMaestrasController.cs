using BaseConLogin.Data;
using BaseConLogin.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Authorize(Roles = "Admin")]
public class PropiedadesMaestrasController : Controller
{
    private readonly ApplicationDbContext _context;

    public PropiedadesMaestrasController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        // En un entorno multi-tenant, aquí filtrarías por TiendaId
        return View(await _context.PropiedadesMaestras.ToListAsync<PropiedadExtendidaMaestra>());
    }

    [HttpPost]
    public async Task<IActionResult> Crear(string nombre, bool esConfigurable)
    {
        if (!string.IsNullOrEmpty(nombre))
        {
            var nueva = new PropiedadExtendidaMaestra
            {
                Nombre = nombre,
                EsConfigurablePorDefecto = esConfigurable,
                TiendaId = 1 // Ajustar a tu lógica de Tenant
            };
            _context.PropiedadesMaestras.Add(nueva);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }
}