using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Netcom.Models
{
    public class AdminResumen
    {
        public int TotalProductos { get; set; }
        public int ProductosAgotados { get; set; }
        public int StockTotal { get; set; }
        public int UsuariosRegistrados { get; set; }
        public int VentasRealizadas { get; set; }
        public decimal TotalRecaudado { get; set; }
    }
}