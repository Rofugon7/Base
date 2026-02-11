using BaseConLogin.Data;
using BaseConLogin.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BaseConLogin.Services.Facturas
{
    public class FacturacionService : IFacturacionService
    {

        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<ApplicationUser> _userManager;
        

        public FacturacionService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
        }

        public async Task<Factura> GenerarFacturaDesdePedido(int pedidoId)
        {
            var pedido = await _context.Pedidos
                .Include(p => p.Items)
                .FirstOrDefaultAsync(p => p.Id == pedidoId);

            if (pedido == null) return null;

            // 1. Crear la cabecera
            var factura = new Factura
            {
                PedidoId = pedido.Id,
                FechaEmision = DateTime.Now,
                NumeroFactura = $"F-{DateTime.Now.Year}-{pedido.Id.ToString("D4")}",
                NombreCliente = pedido.NombreCompleto, // Viene de tu modelo Pedido
                DireccionFacturacion = $"{pedido.Direccion}, {pedido.Ciudad}",
                BaseImponible = pedido.Total / 1.21m, // Cálculo inverso si el total incluye IVA
                TotalIva = pedido.Total - (pedido.Total / 1.21m),
                TotalFactura = pedido.Total
            };

            // 2. Crear las líneas copiando los datos del pedido
            foreach (var item in pedido.Items)
            {
                factura.Lineas.Add(new FacturaLinea
                {
                    Descripcion = item.NombreProducto,
                    Cantidad = item.Cantidad,
                    PrecioUnitario = item.PrecioUnitario
                });
            }

            _context.Facturas.Add(factura);
            await _context.SaveChangesAsync();

            return factura;
        }
    }
}
