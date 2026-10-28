using System;

namespace Netcom.Models
{
    public class Producto
    {
        public int IdProducto { get; set; }
        public string NombreProducto { get; set; }
        public string Descripcion { get; set; }
        public decimal Precio { get; set; }
        public int Stock { get; set; }
        public string Imagen { get; set; }
        public string Marca { get; set; }
        public string EstadoProducto { get; set; }
        public DateTime FechaPublicacion { get; set; }
        public int IdCategoria { get; set; }
        public string NombreCategoria { get; set; }
    }
}