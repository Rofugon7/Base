namespace BaseConLogin.Services.Pedidos
{
    public interface IPedidoService
    {
        Task<int> CrearPedidoDesdeCarritoAsync(int tiendaId, string userId);
    }

}
