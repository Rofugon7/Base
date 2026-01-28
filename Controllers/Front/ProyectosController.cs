using BaseConLogin.Data;
using BaseConLogin.Models;
using BaseConLogin.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize]
public class ProductoSimplesController : Controller
{
    private readonly ApplicationDbContext _context;

    public ProductoSimplesController(ApplicationDbContext context)
    {
        _context = context;
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProductoSimpleCreateVM model)
    {
        if (!ModelState.IsValid)
            return View(model);

        // 🔹 Obtener TiendaId del usuario
        var tiendaId = ObtenerTiendaIdUsuario();

        // 1️⃣ ProductoBase
        var productoBase = new ProductoBase
        {
            TiendaId = tiendaId,
            Nombre = model.Nombre,
            Descripcion = model.Descripcion,
            PrecioBase = model.PrecioBase,
            TipoProducto = TipoProducto.Simple,
            Activo = true,
            ImagenPrincipal = model.ImagenPrincipal
        };

        _context.ProductosBase.Add(productoBase);
        await _context.SaveChangesAsync();


        // 2️⃣ ProductoSimple
        var productoSimple = new ProductoSimple
        {
            ProductoBaseId = productoBase.Id,
            Stock = model.Stock
        };

        _context.ProductoSimples.Add(productoSimple);
        await _context.SaveChangesAsync();

        return RedirectToAction("Index");
    }


    private int ObtenerTiendaIdUsuario()
    {
        // Ajusta según tu sistema actual
        return int.Parse(User.FindFirst("TiendaId")!.Value);
    }
}
