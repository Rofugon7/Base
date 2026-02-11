using BaseConLogin.Data;
using BaseConLogin.Models;
using BaseConLogin.Models.enumerados;
using BaseConLogin.Services.Tiendas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

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

            ViewBag.PedidosFacturados = await _context.Facturas
    .Select(f => f.PedidoId)
    .ToHashSetAsync();

            return View(pedidos);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,AdministradorTienda")]
        public async Task<IActionResult> CambiarEstado(int id, string nuevoEstado)
        {

            var pedido = await _context.Pedidos
        .Include(p => p.Items)
        .FirstOrDefaultAsync(p => p.Id == id);

            if (pedido == null) return NotFound();

            // 1. Bloqueo si el pedido ya está Finalizado o Cancelado (no se puede mover de ahí)
            if (pedido.Estado == "Finalizado" || pedido.Estado == "Cancelado")
            {
                TempData["ErrorMessage"] = "No se puede modificar un pedido finalizado o cancelado.";
                return RedirectToAction(nameof(Gestion));
            }

            bool cambioValido = false;

            // 2. Lógica de flujo lineal
            switch (pedido.Estado)
            {
                case "Pendiente":
                    // De Pendiente solo puede pasar a Pagado o Cancelado
                    if (nuevoEstado == "Pagado" || nuevoEstado == "Cancelado") cambioValido = true;
                    break;

                case "Pagado":
                    // De Pagado solo puede pasar a Enviado o Cancelado
                    if (nuevoEstado == "Enviado" || nuevoEstado == "Cancelado") cambioValido = true;
                    break;

                case "Enviado":
                    // De Enviado solo puede pasar a Finalizado o Cancelado
                    if (nuevoEstado == "Finalizado" || nuevoEstado == "Cancelado") cambioValido = true;
                    break;
            }

            if (!cambioValido)
            {
                TempData["ErrorMessage"] = $"No se permite pasar de {pedido.Estado} a {nuevoEstado}.";
                return RedirectToAction(nameof(Gestion));
            }

            // 3. Acciones automáticas al cambiar el estado
            if (nuevoEstado == "Pagado")
            {
                // Generar factura automáticamente (usando el método que creamos antes)
                var cliente = await _userManager.FindByIdAsync(pedido.UserId);
                var factura = await GenerarFacturaInterna(pedido.Id, cliente);
                if (factura != null)
                    TempData["SuccessMessage"] = "Pedido marcado como Pagado y Factura generada.";
            }

            // Si se cancela, la lógica de rectificación se manejaría preferiblemente desde 
            // el botón de "Rectificar" que ya creamos, para pedir el motivo legal.

            pedido.Estado = nuevoEstado;
            await _context.SaveChangesAsync();

            if (TempData["SuccessMessage"] == null)
                TempData["SuccessMessage"] = "Estado actualizado correctamente.";

            return RedirectToAction(nameof(Gestion));

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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RepetirPedido(int id)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login", "Account");

            var pedidoAnterior = await _context.Pedidos
                .Include(p => p.Items)
                .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);

            if (pedidoAnterior == null) return NotFound();

            // 1. Buscamos si el usuario ya tiene una cabecera de carrito para esta tienda
            var carrito = await _context.Carritos
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId && c.TiendaId == pedidoAnterior.TiendaId);

            // 2. Si no tiene carrito, lo creamos
            if (carrito == null)
            {
                carrito = new CarritoPersistente
                {
                    UserId = userId,
                    TiendaId = pedidoAnterior.TiendaId,
                    Items = new List<CarritoPersistenteItem>()
                };
                _context.Add(carrito);
                await _context.SaveChangesAsync(); // Guardamos para obtener el Id del carrito
            }

            int productosAgregados = 0;

            foreach (var itemPedido in pedidoAnterior.Items)
            {
                var productoReal = await _context.ProductoSimples
                    .FirstOrDefaultAsync(p => p.ProductoBaseId == itemPedido.ProductoBaseId);

                if (productoReal == null || productoReal.Stock <= 0) continue;

                int cantidadAAnadir = itemPedido.Cantidad;
                if (productoReal.Stock < cantidadAAnadir) cantidadAAnadir = productoReal.Stock;

                // 3. Buscamos si el producto ya está en los ITEMS de ese carrito
                var itemExistente = carrito.Items
                    .FirstOrDefault(i => i.ProductoBaseId == productoReal.ProductoBaseId);

                if (itemExistente != null)
                {
                    if (itemExistente.Cantidad + cantidadAAnadir <= productoReal.Stock)
                        itemExistente.Cantidad += cantidadAAnadir;
                    else
                        itemExistente.Cantidad = productoReal.Stock;
                }
                else
                {
                    // 4. Creamos el item vinculado al ID del carrito que encontramos/creamos arriba
                    var nuevoItem = new CarritoPersistenteItem
                    {
                        CarritoPersistenteId = carrito.Id,
                        ProductoBaseId = productoReal.ProductoBaseId,
                        Cantidad = cantidadAAnadir
                    };
                    _context.CarritoItems.Add(nuevoItem);
                }
                productosAgregados++;
            }

            await _context.SaveChangesAsync();

            if (productosAgregados > 0)
            {
                TempData["SuccessMessage"] = "Productos añadidos al carrito correctamente.";
                return RedirectToAction("Index", "Carrito");
            }

            TempData["ErrorMessage"] = "No se pudo repetir el pedido por falta de stock.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet("DescargarFactura")]
        public async Task<IActionResult> DescargarFactura(int id, bool esRectificativa = false)
        {
            var isAdmin = User.IsInRole("Admin");
            var userId = _userManager.GetUserId(User);

            var factura = await _context.Facturas
                .Include(f => f.Lineas)
                .Include(f => f.FacturaOriginal)
                .Include(f => f.Pedido)
                .FirstOrDefaultAsync(f => f.PedidoId == id
                                     && f.EsRectificativa == esRectificativa
                                     && (f.Pedido.UserId == userId || isAdmin));

            if (factura == null) return NotFound("La factura aún no ha sido generada (el pedido debe estar Pagado).");

            return View("DetalleFactura", factura);
        }

        private string CalcularHashVerifactu(string nifEmisor, string numeroFactura, string fecha, string importe, string hashAnterior)
        {
            // La normativa exige concatenar los campos principales
            // Formato sugerido: NIF|NUMERO|FECHA|IMPORTE|HASH_ANTERIOR
            string cadenaParaHash = $"{nifEmisor}|{numeroFactura}|{fecha}|{importe}|{hashAnterior}";

            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(cadenaParaHash));

                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2")); // Convertir a hexadecimal
                }
                return builder.ToString().ToUpper(); // Verifactu suele usar mayúsculas
            }
        }

        [HttpPost("RectificarFactura")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RectificarFactura(int idPedido, string motivo)
        {
            // 1. Buscamos el pedido con sus items para devolver el stock
            var pedido = await _context.Pedidos
                .Include(p => p.Items)
                .FirstOrDefaultAsync(p => p.Id == idPedido);

            if (pedido == null)
            {
                TempData["ErrorMessage"] = "No se encontró el pedido.";
                return RedirectToAction("Gestion");
            }

            // Usamos una transacción para asegurar que o se hace todo o no se hace nada
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // 2. RESTAURAR STOCK (Independiente de si hay factura o no)
                    foreach (var itemPedido in pedido.Items)
                    {
                        // Buscamos el producto simple para devolver las unidades
                        var productoReal = await _context.ProductoSimples
                            .FirstOrDefaultAsync(ps => ps.ProductoBaseId == itemPedido.ProductoBaseId);

                        if (productoReal != null)
                        {
                            productoReal.Stock += itemPedido.Cantidad;
                            _context.Update(productoReal);
                        }
                    }

                    // 3. LÓGICA DE FACTURACIÓN (Opcional: solo si ya estaba facturado)
                    var facturaOriginal = await _context.Facturas
                        .Include(f => f.Lineas)
                        .FirstOrDefaultAsync(f => f.PedidoId == idPedido && !f.EsRectificativa);

                    if (facturaOriginal != null)
                    {
                        // --- Generamos la rectificativa (Tu lógica de Verifactu) ---
                        var ultimaFactura = await _context.Facturas
                            .OrderByDescending(f => f.FechaEmision).ThenByDescending(f => f.Id)
                            .FirstOrDefaultAsync();

                        string hashPrevio = ultimaFactura?.HashActual ?? "0000000000";
                        string nifEmisor = "B78035599";
                        string numRectificativa = $"R-{DateTime.Now.Year}-{facturaOriginal.Id:D5}";
                        decimal totalNegativo = facturaOriginal.TotalFactura * -1;
                        string importeStr = totalNegativo.ToString("F2").Replace(",", ".");

                        var rectificativa = new Factura
                        {
                            EsRectificativa = true,
                            FacturaOriginalId = facturaOriginal.Id,
                            MotivoRectificacion = motivo,
                            PedidoId = idPedido,
                            FechaEmision = DateTime.Now,
                            NumeroFactura = numRectificativa,
                            NombreCliente = facturaOriginal.NombreCliente,
                            DniCie = facturaOriginal.DniCie,
                            DireccionFacturacion = facturaOriginal.DireccionFacturacion,
                            IvaPorcentaje = facturaOriginal.IvaPorcentaje,
                            BaseImponible = facturaOriginal.BaseImponible * -1,
                            TotalIva = facturaOriginal.TotalIva * -1,
                            TotalFactura = totalNegativo,
                            HashAnterior = hashPrevio,
                            HashCertificado = facturaOriginal.HashCertificado ?? "S/N"
                        };

                        foreach (var linea in facturaOriginal.Lineas)
                        {
                            rectificativa.Lineas.Add(new FacturaLinea
                            {
                                Descripcion = "RECTIFICACIÓN: " + linea.Descripcion,
                                Cantidad = linea.Cantidad * -1,
                                PrecioUnitario = linea.PrecioUnitario
                            });
                        }

                        rectificativa.HashActual = CalcularHashVerifactu(nifEmisor, numRectificativa, DateTime.Now.ToString("dd-MM-yyyy"), importeStr, hashPrevio);
                        rectificativa.QRUrl = $"https://www2.agenciatributaria.gob.es/...&importe={importeStr}&hash={rectificativa.HashActual}";

                        _context.Facturas.Add(rectificativa);
                        TempData["SuccessMessage"] = "Pedido cancelado, stock restaurado y factura rectificativa generada.";
                    }
                    else
                    {
                        TempData["SuccessMessage"] = "Pedido cancelado y stock restaurado correctamente (sin factura previa).";
                    }

                    // 4. CAMBIAR ESTADO DEL PEDIDO
                    pedido.Estado = EstadoPedido.Cancelado;
                    _context.Update(pedido);

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    TempData["ErrorMessage"] = "Error crítico al cancelar el pedido: " + ex.Message;
                }
            }

            return RedirectToAction("Gestion");
        }

        private async Task<Factura> GenerarFacturaInterna(int pedidoId, ApplicationUser user)
        {
            // 1. Verificar si ya existe para evitar duplicados
            var facturaExistente = await _context.Facturas
                .FirstOrDefaultAsync(f => f.PedidoId == pedidoId && !f.EsRectificativa);

            if (facturaExistente != null) return facturaExistente;

            // 2. Cargar pedido con sus ítems
            var pedido = await _context.Pedidos
                .Include(p => p.Items)
                .FirstOrDefaultAsync(p => p.Id == pedidoId);

            if (pedido == null) return null;

            // 3. Lógica Veri*factu (Cálculo de Hash y Correlativo)
            var ultimaFactura = await _context.Facturas
                .OrderByDescending(f => f.FechaEmision).ThenByDescending(f => f.Id)
                .FirstOrDefaultAsync();

            string hashPrevio = ultimaFactura?.HashActual ?? "00000000000000000000000000000000";
            string numFactura = $"F-{DateTime.Now.Year}-{pedido.Id:D5}";
            string importeStr = pedido.Total.ToString("F2").Replace(",", ".");
            string nifEmisor = "B78035599";

            string nuevoHash = CalcularHashVerifactu(nifEmisor, numFactura, DateTime.Now.ToString("dd-MM-yyyy"), importeStr, hashPrevio);

            // 4. Crear objeto Factura
            var factura = new Factura
            {
                PedidoId = pedido.Id,
                FechaEmision = DateTime.Now,
                NumeroFactura = numFactura,
                NombreCliente = pedido.NombreCompleto,
                DniCie = user.NifCif ?? "Sin identificar",
                DireccionFacturacion = $"{pedido.Direccion}, {pedido.Ciudad}",
                IvaPorcentaje = 21,
                TotalFactura = pedido.Total,
                BaseImponible = pedido.Total / 1.21m,
                TotalIva = pedido.Total - (pedido.Total / 1.21m),
                HashCertificado = Guid.NewGuid().ToString().Substring(0, 8),
                HashAnterior = hashPrevio,
                HashActual = nuevoHash,
                QRUrl = $"https://www2.agenciatributaria.gob.es/...&hash={nuevoHash}",
                MotivoRectificacion = string.Empty 
            };

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
            return factura;
        }

        
    }
}