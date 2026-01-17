namespace BaseConLogin.Services.Seguridad
{
    public interface ITiendaAuthorizationService
    {
        Task<bool> PuedeGestionarTiendaAsync(string userId, int tiendaId);
    }
}
