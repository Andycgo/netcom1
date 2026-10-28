using Netcom.Data;
using Netcom.Models;
using System;
using System.Collections.Generic;
using System.Web.Http;

namespace Netcom.Controllers
{
    [RoutePrefix("api/producto")]
    public class ProductoController : ApiController
    {
        [HttpGet]
        [Route("")]
        public IHttpActionResult Get()
        {
            try
            {
                List<Producto> lista = ProductoData.ListarProductos();
                return Ok(lista);
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception("Error al cargar productos desde la base de datos: " + ex.Message));
            }
        }

        [HttpGet]
        [Route("{id:int}")]
        public IHttpActionResult Get(int id)
        {
            try
            {
                Producto producto = ProductoData.ConsultarProducto(id);

                if (producto == null)
                    return NotFound();

                return Ok(producto);
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception("Error al consultar producto: " + ex.Message));
            }
        }

        [HttpPost]
        [Route("")]
        public IHttpActionResult Post([FromBody] Producto producto)
        {
            try
            {
                string error = ValidarProducto(producto);

                if (!string.IsNullOrEmpty(error))
                    return BadRequest(error);

                string mensaje;
                bool respuesta = ProductoData.InsertarProducto(producto, out mensaje);

                if (respuesta)
                    return Ok(mensaje);

                return BadRequest(mensaje);
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception("Error al registrar producto: " + ex.Message));
            }
        }

        [HttpPut]
        [Route("")]
        public IHttpActionResult Put([FromBody] Producto producto)
        {
            try
            {
                string error = ValidarProducto(producto);

                if (!string.IsNullOrEmpty(error))
                    return BadRequest(error);

                if (producto.IdProducto <= 0)
                    return BadRequest("El ID del producto es obligatorio para actualizar.");

                string mensaje;
                bool respuesta = ProductoData.ActualizarProducto(producto, out mensaje);

                if (respuesta)
                    return Ok(mensaje);

                return BadRequest(mensaje);
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception("Error al actualizar producto: " + ex.Message));
            }
        }

        [HttpPut]
        [Route("{id:int}")]
        public IHttpActionResult Put(int id, [FromBody] Producto producto)
        {
            try
            {
                if (producto == null)
                    return BadRequest("Los datos del producto son obligatorios.");

                producto.IdProducto = id;

                string error = ValidarProducto(producto);

                if (!string.IsNullOrEmpty(error))
                    return BadRequest(error);

                string mensaje;
                bool respuesta = ProductoData.ActualizarProducto(producto, out mensaje);

                if (respuesta)
                    return Ok(mensaje);

                return BadRequest(mensaje);
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception("Error al actualizar producto: " + ex.Message));
            }
        }

        [HttpDelete]
        [Route("{id:int}")]
        public IHttpActionResult Delete(int id)
        {
            try
            {
                string mensaje;
                bool respuesta = ProductoData.EliminarProducto(id, out mensaje);

                if (respuesta)
                    return Ok(mensaje);

                return BadRequest(mensaje);
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception("Error al eliminar producto: " + ex.Message));
            }
        }

        private string ValidarProducto(Producto producto)
        {
            if (producto == null)
                return "Los datos del producto son obligatorios.";

            if (string.IsNullOrWhiteSpace(producto.NombreProducto))
                return "El nombre del producto es obligatorio.";

            if (string.IsNullOrWhiteSpace(producto.Marca))
                return "La marca es obligatoria.";

            if (string.IsNullOrWhiteSpace(producto.Descripcion))
                return "La descripción es obligatoria.";

            if (producto.Precio <= 0)
                return "El precio debe ser mayor que cero.";

            if (producto.Stock < 0)
                return "El stock no puede ser negativo.";

            if (producto.IdCategoria <= 0)
                return "Debe seleccionar una categoría válida.";

            return "";
        }
    }
}