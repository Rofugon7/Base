namespace BaseConLogin.Models
{
    public class Pedido
    {
        public int Id { get; set; }

        public string UserId { get; set; }

        public int TiendaId { get; set; }

        public DateTime Fecha { get; set; }

        public decimal Total { get; set; }

        public string Estado { get; set; } // Pendiente, Pagado, Cancelado

        public ICollection<PedidoItem> Items { get; set; }
    }
}
