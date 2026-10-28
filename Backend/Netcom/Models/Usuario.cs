using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Netcom.Models
{
    public class Usuario
    {
        public int IdUsuario { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Correo { get; set; }
        public string Contrasena { get; set; }
        public string Telefono { get; set; }
        public string Direccion { get; set; }
        public string Rol { get; set; }
        public DateTime FechaRegistro { get; set; }
    }
}