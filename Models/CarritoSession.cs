using System.Collections.Generic;
using System.Linq;

namespace BaseConLogin.Models
{
    public class CarritoSession
    {
        public int? TiendaId { get; set; }
        public List<CarritoSessionItem> Items { get; set; } = new List<CarritoSessionItem>();

        public decimal Total =>
            Items.Sum(i => i.PrecioUnitario * i.Cantidad);
    }
}
