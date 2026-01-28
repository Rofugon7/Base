namespace BaseConLogin.Services.Tiendas
{
    public interface ITiendaContext
    {
        int TiendaId { get; }

        string? Slug { get; }

        bool HayTienda { get; }

        void EstablecerTienda(int tiendaId, string slug);

        void LimpiarTienda();

        int ObtenerTiendaId();

        int? ObtenerTiendaIdOpcional();

        string? ObtenerSlug();

        Task<int?> ObtenerTiendaIdUsuarioAsync(string userId);
    }
}
