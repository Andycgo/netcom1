using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Netcom.Models
{
    public class VentaRequest
    {
        public int IdUsuario { get; set; }
        public string MetodoPago { get; set; }
        public string DatoPago { get; set; }
        public List<VentaDetalle> Detalles { get; set; }
    }
}
