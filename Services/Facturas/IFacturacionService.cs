using BaseConLogin.Models;

namespace BaseConLogin.Services.Facturas
{
    public interface IFacturacionService
    {
        Task<Factura> GenerarFacturaDesdePedido(int pedidoId);
    }
}
