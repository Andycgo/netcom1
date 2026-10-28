using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Netcom.Models
{
    public class AdminVentaDetalle
    {
        public int IdDetalle { get; set; }
        public int IdVenta { get; set; }
        public string Producto { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }
    }
}