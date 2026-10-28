using System;

namespace Netcom.Models
{
    public class AdminReporteVenta
    {
        public int IdVenta { get; set; }
        public DateTime FechaVenta { get; set; }
        public string NombreCliente { get; set; }
        public string CorreoCliente { get; set; }
        public string MetodoPago { get; set; }
        public string Estado { get; set; }
        public string NombreProducto { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }
        public decimal TotalVenta { get; set; }
    }
}