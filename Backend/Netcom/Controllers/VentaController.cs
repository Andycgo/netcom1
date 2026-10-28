using Netcom.Data;
using Netcom.Models;
using System.Web.Http;

namespace Netcom.Controllers
{
    [RoutePrefix("api/venta")]
    public class VentaController : ApiController
    {
        // POST: api/venta
        [HttpPost]
        [Route("")]
        public IHttpActionResult Post([FromBody] VentaRequest venta)
        {
            string mensaje;
            int idVenta = VentaData.RegistrarVenta(venta, out mensaje);

            if (idVenta > 0)
            {
                return Ok(new
                {
                    IdVenta = idVenta,
                    Mensaje = mensaje
                });
            }

            return BadRequest(mensaje);
        }

        // GET: api/venta/resumen/5
        [HttpGet]
        [Route("resumen/{idUsuario:int}")]
        public IHttpActionResult Resumen(int idUsuario)
        {
            VentaResumen resumen = VentaData.ObtenerResumenUsuario(idUsuario);

            if (resumen == null)
                return NotFound();

            return Ok(resumen);
        }
    }
}