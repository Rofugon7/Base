using BaseConLogin.Data;
using BaseConLogin.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace BaseConLogin.Services.Carritos
{
    public class DbCarritoService : ICarritoService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<ApplicationUser> _userManager;
        private const string SessionKey = "CARRITO_TIENDA_";

        public DbCarritoService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
        }

        // ==========================================
        // CONTEO DE ITEMS (SIN ERRORES DE MODELO)
        // ==========================================
        public async Task<int> ObtenerCantidadItemsAsync(int tiendaId)
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user != null && user.Identity!.IsAuthenticated)
            {
                var userId = _userManager.GetUserId(user);

                // Consultamos el carrito incluyendo los items para sumar
                var carrito = await _context.Carritos
                    .AsNoTracking() // Mejora rendimiento y evita bloqueos
                    .Include(c => c.Items)
                    .FirstOrDefaultAsync(c => c.UserId == userId && (tiendaId == 0 || c.TiendaId == tiendaId));

                return carrito?.Items.Sum(i => i.Cantidad) ?? 0;
            }

            // Caso invitado
            return ObtenerCarritoSesion(tiendaId).Items.Sum(i => i.Cantidad);
        }

        public async Task AñadirProductoAsync(int tiendaId, int productoBaseId, int cantidad = 1, decimal? precioPersonalizado = null, string detallesOpciones = "")
        {
            var producto = await _context.ProductosBase.AsNoTracking().FirstOrDefaultAsync(p => p.Id == productoBaseId);
            if (producto == null) return;
            if (tiendaId <= 0) tiendaId = producto.TiendaId;

            var productoSimple = await _context.ProductosBase.FirstOrDefaultAsync(ps => ps.Id == productoBaseId);
            if (productoSimple == null || productoSimple.Stock <= 0) return;

            if (cantidad > productoSimple.Stock) cantidad = productoSimple.Stock;

            // Determinamos qué precio usar: el personalizado de la vista o el base del producto
            decimal precioAFijar = precioPersonalizado ?? producto.PrecioBase;

            var user = _httpContextAccessor.HttpContext?.User;
            if (user != null && user.Identity!.IsAuthenticated)
            {
                var userId = _userManager.GetUserId(user);
                var cp = await _context.Carritos.Include(c => c.Items).FirstOrDefaultAsync(c => c.UserId == userId && c.TiendaId == tiendaId);

                if (cp == null)
                {
                    cp = new CarritoPersistente { UserId = userId, TiendaId = tiendaId };
                    _context.Carritos.Add(cp);
                }

                // Buscamos si ya existe el MISMO producto con las MISMAS opciones
                // (Si las opciones cambian, lo tratamos como un item nuevo en el carrito)
                var existente = cp.Items.FirstOrDefault(i =>
                    i.ProductoBaseId == productoBaseId &&
                    (i.DetallesOpciones == detallesOpciones)); // Asumiendo que añades esta prop a tu modelo

                if (existente == null)
                {
                    cp.Items.Add(new CarritoPersistenteItem
                    {
                        ProductoBaseId = productoBaseId,
                        Cantidad = cantidad,
                        PrecioPersonalizado = precioAFijar, // Debes añadir esta prop al modelo
                        DetallesOpciones = detallesOpciones // Debes añadir esta prop al modelo
                    });
                }
                else
                {
                    existente.Cantidad += cantidad;
                }

                await _context.SaveChangesAsync();
            }
            else
            {
                // MODO INVITADO (SESIÓN)
                var carrito = ObtenerCarritoSesion(tiendaId);

                // En sesión es más fácil: buscamos por ID y opciones
                var item = carrito.Items.FirstOrDefault(i =>
                    i.ProductoBaseId == productoBaseId &&
                    i.OpcionesSeleccionadas == detallesOpciones);

                if (item == null)
                {
                    carrito.Items.Add(new CarritoSessionItem
                    {
                        ProductoBaseId = productoBaseId,
                        Nombre = producto.Nombre,
                        PrecioUnitario = precioAFijar, // Aquí usamos el precio calculado
                        Cantidad = cantidad,
                        ImagenRuta = producto.ImagenPrincipal ?? "",
                        OpcionesSeleccionadas = detallesOpciones // Asegúrate de que CarritoSessionItem tenga esta prop
                    });
                }
                else
                {
                    item.Cantidad += cantidad;
                }
                GuardarCarritoSesion(tiendaId, carrito);
            }
        }


        public async Task LimpiarCarritoAsync(int tiendaId)
        {
            // 1. Obtenemos el usuario actual
            var userId = _userManager.GetUserId(_httpContextAccessor.HttpContext.User);

            // 2. Buscamos el ID del carrito en la base de datos para este usuario y tienda
            var carritoId = await _context.Carritos // Asumiendo que tu tabla se llama Carritos o CarritosPersistentes
                .IgnoreQueryFilters()
                .Where(c => c.UserId == userId && c.TiendaId == tiendaId)
                .Select(c => c.Id)
                .FirstOrDefaultAsync();

            if (carritoId > 0)
            {
                // 3. Borramos los items usando ese ID
                var items = await _context.CarritoItems
                    .IgnoreQueryFilters()
                    .Where(i => i.CarritoPersistenteId == carritoId) // O el nombre que use tu FK
                    .ToListAsync();

                if (items.Any())
                {
                    _context.CarritoItems.RemoveRange(items);
                    await _context.SaveChangesAsync();
                }
            }

            // 4. Limpiar rastro en la sesión
            _httpContextAccessor.HttpContext?.Session.Remove("Carrito");
        }


        public async Task<CarritoSession> ObtenerCarritoAsync(int tiendaId)
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user != null && user.Identity!.IsAuthenticated)
            {
                await FusionarCarritoSesionAsync(tiendaId); // Sincroniza sesión y DB
                var userId = _userManager.GetUserId(user);

                // Cargamos el carrito incluyendo explícitamente los Items
                var query = _context.Carritos.Include(c => c.Items).AsQueryable();

                CarritoPersistente? cp;
                if (tiendaId > 0)
                {
                    cp = await query.FirstOrDefaultAsync(c => c.UserId == userId && c.TiendaId == tiendaId);
                }
                else
                {
                    // Si es 0, tomamos el primer carrito que tenga productos del usuario
                    cp = await query.OrderByDescending(c => c.Id)
                                    .FirstOrDefaultAsync(c => c.UserId == userId && c.Items.Any());
                }

                if (cp == null) return new CarritoSession { TiendaId = tiendaId };

                return await MapCarritoAsync(cp);
            }

            return ObtenerCarritoSesion(tiendaId);
        }

        public async Task FusionarCarritoSesionAsync(int tiendaId)
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            var json = session?.GetString(SessionKey + tiendaId);
            if (string.IsNullOrEmpty(json)) return;

            var carritoSesion = JsonSerializer.Deserialize<CarritoSession>(json);
            if (carritoSesion == null || !carritoSesion.Items.Any()) return;

            var userId = _userManager.GetUserId(_httpContextAccessor.HttpContext!.User);
            var cp = await _context.Carritos.Include(c => c.Items).FirstOrDefaultAsync(c => c.UserId == userId && c.TiendaId == tiendaId);

            if (cp == null)
            {
                cp = new CarritoPersistente { UserId = userId, TiendaId = tiendaId };
                _context.Carritos.Add(cp);
            }

            foreach (var item in carritoSesion.Items)
            {
                var ex = cp.Items.FirstOrDefault(i => i.ProductoBaseId == item.ProductoBaseId);
                if (ex == null) cp.Items.Add(new CarritoPersistenteItem { ProductoBaseId = item.ProductoBaseId, Cantidad = item.Cantidad });
                else ex.Cantidad += item.Cantidad;
            }

            await _context.SaveChangesAsync();
            session!.Remove(SessionKey + tiendaId);
        }

        private CarritoSession ObtenerCarritoSesion(int tiendaId)
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            if (session == null) return new CarritoSession { TiendaId = tiendaId };

            // 1. Intentar cargar la tienda específica
            var json = session.GetString(SessionKey + tiendaId);

            // 2. Si es tienda 0 y está vacía, buscamos CUALQUIER sesión de carrito activa
            if (string.IsNullOrEmpty(json) && tiendaId == 0)
            {
                // Buscamos en las llaves de sesión alguna que empiece por nuestra constante
                var llaveExistente = session.Keys.FirstOrDefault(k => k.StartsWith(SessionKey));
                if (llaveExistente != null) json = session.GetString(llaveExistente);
            }

            if (string.IsNullOrEmpty(json)) return new CarritoSession { TiendaId = tiendaId };

            return JsonSerializer.Deserialize<CarritoSession>(json) ?? new CarritoSession { TiendaId = tiendaId };
        }

        private void GuardarCarritoSesion(int tiendaId, CarritoSession carrito) =>
            _httpContextAccessor.HttpContext?.Session.SetString(SessionKey + tiendaId, JsonSerializer.Serialize(carrito));

        private async Task<CarritoSession> MapCarritoAsync(CarritoPersistente cp)
        {
            var ids = cp.Items.Select(i => i.ProductoBaseId).ToList();
            var productos = await _context.ProductosBase.AsNoTracking()
                .Where(p => ids.Contains(p.Id)).ToDictionaryAsync(p => p.Id);

            var carrito = new CarritoSession { TiendaId = cp.TiendaId };

            foreach (var item in cp.Items)
            {
                if (productos.TryGetValue(item.ProductoBaseId, out var p))
                {
                    carrito.Items.Add(new CarritoSessionItem
                    {
                        ProductoBaseId = p.Id,
                        Nombre = p.Nombre,
                        // PRIORIDAD: Si hay un precio guardado en el item del carrito, lo usamos
                        PrecioUnitario = item.PrecioPersonalizado > 0 ? item.PrecioPersonalizado : p.PrecioBase,
                        Cantidad = item.Cantidad,
                        ImagenRuta = p.ImagenPrincipal,
                        OpcionesSeleccionadas = item.DetallesOpciones // Pasamos las opciones guardadas
                    });
                }
            }
            return carrito;
        }


        // Métodos de interfaz para que no falle la compilación
        public async Task ActualizarCantidadAsync(int tiendaId, int productoBaseId, int cantidad)
        {
            var user = _httpContextAccessor.HttpContext?.User;

            if (user != null && user.Identity!.IsAuthenticated)
            {
                var userId = _userManager.GetUserId(user);

                // Buscamos el carrito padre primero para evitar el error de definición en el item
                var carrito = await _context.Carritos
                    .Include(c => c.Items)
                    .FirstOrDefaultAsync(c => c.UserId == userId && c.TiendaId == tiendaId);

                if (carrito != null)
                {
                    var item = carrito.Items.FirstOrDefault(i => i.ProductoBaseId == productoBaseId);
                    if (item != null)
                    {
                        if (cantidad <= 0)
                            _context.CarritoItems.Remove(item);
                        else
                            item.Cantidad = cantidad;

                        await _context.SaveChangesAsync();
                    }
                }
            }
            else
            {
                // MODO INVITADO (Sesión)
                var carrito = ObtenerCarritoSesion(tiendaId);
                var item = carrito.Items.FirstOrDefault(i => i.ProductoBaseId == productoBaseId);

                if (item != null)
                {
                    if (cantidad <= 0)
                        carrito.Items.Remove(item);
                    else
                        item.Cantidad = cantidad;

                    GuardarCarritoSesion(tiendaId, carrito);
                }
            }
        }

              public async Task EliminarProductoAsync(int tiendaId, int productoBaseId)
        {
            // Reutilizamos la lógica de actualizar con cantidad 0
            await ActualizarCantidadAsync(tiendaId, productoBaseId, 0);
        }
        public async Task VaciarAsync(int tiendaId)
        {
            var user = _httpContextAccessor.HttpContext?.User;

            if (user != null && user.Identity!.IsAuthenticated)
            {
                var userId = _userManager.GetUserId(user);
                var carrito = await _context.Carritos
                    .Include(c => c.Items)
                    .FirstOrDefaultAsync(c => c.UserId == userId && c.TiendaId == tiendaId);

                if (carrito != null)
                {
                    _context.CarritoItems.RemoveRange(carrito.Items);
                    await _context.SaveChangesAsync();
                }
            }
            else
            {
                // MODO INVITADO: Simplemente eliminamos la llave de la sesión
                _httpContextAccessor.HttpContext?.Session.Remove(SessionKey + tiendaId);
            }
        }
        public Task<int> ObtenerCantidadItemsAsync(string u) => Task.FromResult(0);
    }
}