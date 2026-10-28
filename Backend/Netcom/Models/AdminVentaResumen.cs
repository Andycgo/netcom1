using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Netcom.Models
{
    public class AdminVentaResumen
    {
        public int IdVenta { get; set; }
        public int IdUsuario { get; set; }
        public string Cliente { get; set; }
        public string Correo { get; set; }
        public DateTime FechaVenta { get; set; }
        public string MetodoPago { get; set; }
        public decimal Total { get; set; }
        public string Estado { get; set; }
        public int CantidadProductos { get; set; }
    }
}