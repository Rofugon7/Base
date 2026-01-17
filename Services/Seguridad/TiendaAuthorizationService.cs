using BaseConLogin.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BaseConLogin.Services.Seguridad
{
    public class TiendaAuthorizationService : ITiendaAuthorizationService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public TiendaAuthorizationService(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<bool> PuedeGestionarTiendaAsync(string userId, int tiendaId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return false;

            if (await _userManager.IsInRoleAsync(user, "Admin"))
                return true;

            if (await _userManager.IsInRoleAsync(user, "AdministradorTienda"))
            {
                return await _context.UsuariosTiendas
                    .AnyAsync(ut => ut.UserId == userId && ut.TiendaId == tiendaId);
            }

            return false;
        }
    }
}
