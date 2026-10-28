using ClosedXML.Excel;
using Netcom.Data;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;

namespace Netcom.Controllers
{
    [RoutePrefix("api/admin")]
    public class AdminController : ApiController
    {
        [HttpGet]
        [Route("resumen")]
        public IHttpActionResult ObtenerResumen()
        {
            try
            {
                var resumen = AdminData.ObtenerResumen();
                return Ok(resumen);
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception("Error al obtener el resumen: " + ex.Message));
            }
        }

        [HttpGet]
        [Route("usuarios")]
        public IHttpActionResult ObtenerUsuarios()
        {
            try
            {
                var usuarios = AdminData.ObtenerUsuarios();
                return Ok(usuarios);
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception("Error al obtener usuarios: " + ex.Message));
            }
        }

        [HttpGet]
        [Route("ventas")]
        public IHttpActionResult ObtenerVentas(int limite = 100)
        {
            try
            {
                if (limite <= 0 || limite > 500)
                {
                    limite = 100;
                }

                var ventas = AdminData.ObtenerVentas(limite);
                return Ok(ventas);
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception("Error al obtener ventas: " + ex.Message));
            }
        }

        [HttpGet]
        [Route("ventas/{idVenta:int}")]
        public IHttpActionResult ObtenerDetalleVenta(int idVenta)
        {
            try
            {
                if (idVenta <= 0)
                {
                    return BadRequest("El id de la venta no es válido.");
                }

                var detalle = AdminData.ObtenerDetalleVenta(idVenta);

                if (detalle == null || detalle.Count == 0)
                {
                    return NotFound();
                }

                return Ok(detalle);
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception("Error al obtener detalle de venta: " + ex.Message));
            }
        }

        [HttpGet]
        [Route("grafica-ventas")]
        public IHttpActionResult ObtenerGraficaVentas(int? idUsuario = null)
        {
            try
            {
                var datos = AdminData.ObtenerGraficaVentas(idUsuario);
                return Ok(datos);
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception("Error al obtener gráfica de ventas: " + ex.Message));
            }
        }

        [HttpGet]
        [Route("reporte-ventas")]
        public HttpResponseMessage DescargarReporteVentas(DateTime desde, DateTime hasta)
        {
            try
            {
                desde = desde.Date;
                hasta = hasta.Date;

                if (desde > hasta)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, new
                    {
                        mensaje = "La fecha inicial no puede ser mayor que la fecha final."
                    });
                }

                if (hasta > desde.AddMonths(6))
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, new
                    {
                        mensaje = "El rango del reporte no puede superar los 6 meses."
                    });
                }

                DateTime hastaExclusivo = hasta.AddDays(1);

                var reporte = AdminData.ObtenerReporteVentas(desde, hastaExclusivo);

                if (reporte == null || reporte.Count == 0)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, new
                    {
                        mensaje = "No hay ventas registradas en el periodo seleccionado."
                    });
                }

                using (var workbook = new XLWorkbook())
                {
                    var hoja = workbook.Worksheets.Add("Ventas");

                    hoja.Cell(1, 1).Value = "ID Venta";
                    hoja.Cell(1, 2).Value = "Fecha";
                    hoja.Cell(1, 3).Value = "Cliente";
                    hoja.Cell(1, 4).Value = "Correo";
                    hoja.Cell(1, 5).Value = "Método de pago";
                    hoja.Cell(1, 6).Value = "Estado";
                    hoja.Cell(1, 7).Value = "Producto";
                    hoja.Cell(1, 8).Value = "Cantidad";
                    hoja.Cell(1, 9).Value = "Precio unitario";
                    hoja.Cell(1, 10).Value = "Subtotal";
                    hoja.Cell(1, 11).Value = "Total venta";

                    var encabezado = hoja.Range(1, 1, 1, 11);
                    encabezado.Style.Font.Bold = true;

                    int fila = 2;

                    foreach (var item in reporte)
                    {
                        hoja.Cell(fila, 1).Value = item.IdVenta;
                        hoja.Cell(fila, 2).Value = item.FechaVenta;
                        hoja.Cell(fila, 3).Value = item.NombreCliente;
                        hoja.Cell(fila, 4).Value = item.CorreoCliente;
                        hoja.Cell(fila, 5).Value = item.MetodoPago;
                        hoja.Cell(fila, 6).Value = item.Estado;
                        hoja.Cell(fila, 7).Value = item.NombreProducto;
                        hoja.Cell(fila, 8).Value = item.Cantidad;
                        hoja.Cell(fila, 9).Value = item.PrecioUnitario;
                        hoja.Cell(fila, 10).Value = item.Subtotal;
                        hoja.Cell(fila, 11).Value = item.TotalVenta;

                        fila++;
                    }

                    hoja.Column(2).Style.DateFormat.Format = "yyyy-mm-dd hh:mm";
                    hoja.Column(9).Style.NumberFormat.Format = "$ #,##0";
                    hoja.Column(10).Style.NumberFormat.Format = "$ #,##0";
                    hoja.Column(11).Style.NumberFormat.Format = "$ #,##0";

                    hoja.Columns(1, 11).Width = 18;
                    hoja.Column(3).Width = 28;
                    hoja.Column(4).Width = 32;
                    hoja.Column(7).Width = 35;

                    byte[] archivoBytes;

                    using (var stream = new MemoryStream())
                    {
                        workbook.SaveAs(stream);
                        archivoBytes = stream.ToArray();
                    }

                    string nombreArchivo = $"reporte_ventas_{desde:yyyyMMdd}_{hasta:yyyyMMdd}.xlsx";

                    var response = new HttpResponseMessage(HttpStatusCode.OK);
                    response.Content = new ByteArrayContent(archivoBytes);

                    response.Content.Headers.ContentType = new MediaTypeHeaderValue(
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
                    );

                    response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                    {
                        FileName = nombreArchivo
                    };

                    return response;
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new
                {
                    mensaje = "Error al generar el reporte de ventas.",
                    detalle = ex.Message
                });
            }
        }
    }
}