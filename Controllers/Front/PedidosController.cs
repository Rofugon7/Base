using BaseConLogin.Data;
using BaseConLogin.Models;
using BaseConLogin.Services.Tiendas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BaseConLogin.Models.enumerados;

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
        public async Task<IActionResult> Gestion(string buscarNombre, string filtrarEstado)
        {
            var user = await _userManager.GetUserAsync(User);
            var usuarioTienda = await _context.UsuariosTiendas
                .FirstOrDefaultAsync(ut => ut.UserId == user.Id);

            if (usuarioTienda == null && !User.IsInRole("Admin")) return Forbid();

            IQueryable<Pedido> query = _context.Pedidos.IgnoreQueryFilters();

            // 1. Filtro por Tienda (Seguridad)
            if (usuarioTienda != null)
            {
                query = query.Where(p => p.TiendaId == usuarioTienda.TiendaId);
            }

            // 2. Filtro por Nombre de Cliente
            if (!string.IsNullOrEmpty(buscarNombre))
            {
                query = query.Where(p => p.NombreCompleto.Contains(buscarNombre));
            }

            // 3. Filtro por Estado
            if (!string.IsNullOrEmpty(filtrarEstado))
            {
                query = query.Where(p => p.Estado == filtrarEstado);
            }

            var pedidos = await query.OrderByDescending(p => p.Fecha).ToListAsync();

            // CALCULAMOS LAS MÉTRICAS
            // 1. Total de ingresos de pedidos Finalizados
            ViewBag.TotalVentas = pedidos
                .Where(p => p.Estado == EstadoPedido.Finalizado)
                .Sum(p => p.Total);

            // 2. Tiempo medio de entrega (en horas)
            var pedidosFinalizados = pedidos.Where(p => p.FechaFinalizacion.HasValue).ToList();
            if (pedidosFinalizados.Any())
            {
                var promedioHoras = pedidosFinalizados
                    .Average(p => (p.FechaFinalizacion.Value - p.Fecha).TotalHours);
                ViewBag.TiempoPromedio = Math.Round(promedioHoras, 1);
            }
            else
            {
                ViewBag.TiempoPromedio = 0;
            }

            // Guardamos los valores para que los filtros mantengan el estado en la vista
            ViewBag.NombreActual = buscarNombre;
            ViewBag.EstadoActual = filtrarEstado;

            return View(pedidos);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,AdministradorTienda")]
        public async Task<IActionResult> CambiarEstado(int id, string nuevoEstado)
        {
            var pedido = await _context.Pedidos.FindAsync(id);
            if (pedido == null) return NotFound();

            pedido.Estado = nuevoEstado;

            // Si se mueve a Finalizado, registramos la fecha
            if (nuevoEstado == EstadoPedido.Finalizado)
            {
                pedido.FechaFinalizacion = DateTime.Now;
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"El pedido #{id} ahora está {nuevoEstado}.";
            return RedirectToAction(nameof(Gestion), new { buscarNombre = ViewBag.NombreActual, filtrarEstado = ViewBag.EstadoActual });
        }

        [Authorize(Roles = "Admin,AdministradorTienda")]
        [HttpPost]
        public async Task<IActionResult> MarcarComoFinalizado(int id)
        {
            var pedido = await _context.Pedidos
                .IgnoreQueryFilters() // Para que el admin lo encuentre siempre
                .FirstOrDefaultAsync(p => p.Id == id);

            if (pedido == null) return NotFound();

            // Regla de negocio: Solo podemos finalizar si ya fue enviado
            if (pedido.Estado == EstadoPedido.Enviado)
            {
                pedido.Estado = EstadoPedido.Finalizado;
                pedido.FechaFinalizacion = DateTime.Now; // Es bueno tener esta auditoría

                await _context.SaveChangesAsync();
                TempData["Success"] = "El pedido ha sido marcado como entregado y finalizado.";
            }
            else
            {
                TempData["Error"] = "Solo se pueden finalizar pedidos que estén en estado 'Enviado'.";
            }

            return RedirectToAction("Detalle", new { id = pedido.Id });
        }
    }
}