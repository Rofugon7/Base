using System.Collections.Generic;
using System.Linq;

namespace BaseConLogin.Models
{
    public class Carrito
    {
        public List<CarritoItem> Items { get; set; } = new();

        public decimal Total =>
            Items.Sum(i => i.PrecioUnitario * i.Cantidad);
    }
}
