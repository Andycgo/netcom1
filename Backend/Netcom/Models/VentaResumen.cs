using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Netcom.Models
{
    public class VentaResumen
    {
        public int IdUsuario { get; set; }
        public string NombreCompleto { get; set; }
        public string Correo { get; set; }
        public string Rol { get; set; }
        public DateTime FechaRegistro { get; set; }
        public int ComprasRealizadas { get; set; }
        public decimal TotalComprado { get; set; }
        public DateTime? UltimaCompra { get; set; }
    }
}