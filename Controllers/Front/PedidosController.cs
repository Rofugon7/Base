using BaseConLogin.Data;
using BaseConLogin.Models;
using BaseConLogin.Services.Tiendas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BaseConLogin.Controllers.Front
{
    [Authorize] // Solo usuarios logueados
    public class PedidosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITiendaContext _tiendaContext;

        public PedidosController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, ITiendaContext tiendaContext)
        {
            _context = context;
            _userManager = userManager;
            _tiendaContext = tiendaContext;
        }

        // Listado de todos los pedidos del usuario
        public async Task<IActionResult> MisPedidos()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var pedidos = await _context.Pedidos
                .Where(p => p.UserId == user.Id)
                .OrderByDescending(p => p.Fecha)
                .ToListAsync();

            return View(pedidos);
        }

        // Detalle de un pedido específico
        public async Task<IActionResult> Detalle(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            var pedido = await _context.Pedidos
                .Include(p => p.Items)
                .FirstOrDefaultAsync(p => p.Id == id && p.UserId == user.Id);

            if (pedido == null) return NotFound();

            return View(pedido);
        }

        // Añade este método a tu PedidosController.cs existente

        [Authorize(Roles = "Admin,AdministradorTienda")]
        public async Task<IActionResult> Gestion()
        {
            // En lugar de confiar en la URL, buscamos qué tienda gestiona este usuario
            var user = await _userManager.GetUserAsync(User);
            var usuarioTienda = await _context.UsuariosTiendas
                .FirstOrDefaultAsync(ut => ut.UserId == user.Id);

            if (usuarioTienda == null && !User.IsInRole("Admin")) return Forbid();

            IQueryable<Pedido> query = _context.Pedidos.IgnoreQueryFilters();

            // Si es admin de una tienda específica, filtramos por la suya
            if (usuarioTienda != null)
            {
                query = query.Where(p => p.TiendaId == usuarioTienda.TiendaId);
            }

            var pedidos = await query.OrderByDescending(p => p.Fecha).ToListAsync();
            return View(pedidos);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,AdministradorTienda")]
        public async Task<IActionResult> CambiarEstado(int id, string nuevoEstado)
        {
            var pedido = await _context.Pedidos.FindAsync(id);
            if (pedido == null) return NotFound();

            pedido.Estado = nuevoEstado;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"El pedido #{id} ahora está {nuevoEstado}.";
            return RedirectToAction(nameof(Gestion));
        }
    }
}