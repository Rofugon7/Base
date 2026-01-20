namespace BaseConLogin.Services.Tiendas
{
    public interface ITiendaContext
    {
        int TiendaId { get; }
        bool HayTienda { get; }

        void EstablecerTienda(int tiendaId);
        void LimpiarTienda();
    }
}
