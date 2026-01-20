using System.Collections.Generic;

namespace BaseConLogin.Models
{
    public class Carrito
    {
        public int TiendaId { get; set; }
        public List<CarritoItem> Items { get; set; } = new List<CarritoItem>();
    }
}
