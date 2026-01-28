using BaseConLogin.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

public class TiendaClaimsTransformation : IClaimsTransformation
{
    private readonly ApplicationDbContext _context;

    public TiendaClaimsTransformation(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        if (!principal.Identity?.IsAuthenticated ?? true)
            return principal;

        if (principal.HasClaim(c => c.Type == "TiendaId"))
            return principal;

        var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
            return principal;

        var tiendaId = await _context.UsuariosTiendas
            .Where(x => x.UserId == userId)
            .Select(x => x.TiendaId)
            .FirstOrDefaultAsync();

        if (tiendaId == 0)
            return principal;

        var identity = (ClaimsIdentity)principal.Identity;

        identity.AddClaim(new Claim("TiendaId", tiendaId.ToString()));

        return principal;
    }
}
