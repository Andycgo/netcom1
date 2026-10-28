using Netcom.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Netcom.Data
{
    public class AdminData
    {
        private static string conexion =
            @"Data Source=MILLER\SQLEXPRESS;Initial Catalog=netcom;Integrated Security=True";

        public static AdminResumen ObtenerResumen()
        {
            AdminResumen resumen = new AdminResumen();

            using (SqlConnection con = new SqlConnection(conexion))
            {
                SqlCommand cmd = new SqlCommand(@"
                    SELECT
                        (SELECT COUNT(*) FROM productos) AS total_productos,
                        (SELECT COUNT(*) FROM productos WHERE stock <= 0) AS productos_agotados,
                        (SELECT ISNULL(SUM(stock), 0) FROM productos) AS stock_total,
                        (SELECT COUNT(*) FROM usuarios) AS usuarios_registrados,
                        (SELECT COUNT(*) FROM ventas) AS ventas_realizadas,
                        (SELECT ISNULL(SUM(total), 0) FROM ventas) AS total_recaudado;
                ", con);

                cmd.CommandType = CommandType.Text;

                con.Open();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        resumen.TotalProductos = Convert.ToInt32(dr["total_productos"]);
                        resumen.ProductosAgotados = Convert.ToInt32(dr["productos_agotados"]);
                        resumen.StockTotal = Convert.ToInt32(dr["stock_total"]);
                        resumen.UsuariosRegistrados = Convert.ToInt32(dr["usuarios_registrados"]);
                        resumen.VentasRealizadas = Convert.ToInt32(dr["ventas_realizadas"]);
                        resumen.TotalRecaudado = Convert.ToDecimal(dr["total_recaudado"]);
                    }
                }
            }

            return resumen;
        }

        public static List<AdminUsuarioResumen> ObtenerUsuarios()
        {
            List<AdminUsuarioResumen> lista = new List<AdminUsuarioResumen>();

            using (SqlConnection con = new SqlConnection(conexion))
            {
                SqlCommand cmd = new SqlCommand(@"
                    SELECT
                        u.id_usuario,
                        LTRIM(RTRIM(ISNULL(u.nombre, '') + ' ' + ISNULL(u.apellido, ''))) AS nombre_completo,
                        u.correo,
                        u.rol,
                        u.fecha_registro,
                        COUNT(v.id_venta) AS compras_realizadas,
                        ISNULL(SUM(v.total), 0) AS total_comprado,
                        MAX(v.fecha_venta) AS ultima_compra
                    FROM usuarios u
                    LEFT JOIN ventas v ON u.id_usuario = v.id_usuario
                    GROUP BY
                        u.id_usuario,
                        u.nombre,
                        u.apellido,
                        u.correo,
                        u.rol,
                        u.fecha_registro
                    ORDER BY u.id_usuario DESC;
                ", con);

                cmd.CommandType = CommandType.Text;

                con.Open();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        lista.Add(new AdminUsuarioResumen()
                        {
                            IdUsuario = Convert.ToInt32(dr["id_usuario"]),
                            NombreCompleto = dr["nombre_completo"].ToString(),
                            Correo = dr["correo"].ToString(),
                            Rol = dr["rol"].ToString(),
                            FechaRegistro = dr["fecha_registro"] == DBNull.Value
                                ? DateTime.Now
                                : Convert.ToDateTime(dr["fecha_registro"]),
                            ComprasRealizadas = Convert.ToInt32(dr["compras_realizadas"]),
                            TotalComprado = Convert.ToDecimal(dr["total_comprado"]),
                            UltimaCompra = dr["ultima_compra"] == DBNull.Value
                                ? (DateTime?)null
                                : Convert.ToDateTime(dr["ultima_compra"])
                        });
                    }
                }
            }

            return lista;
        }

        public static List<AdminVentaResumen> ObtenerVentas(int limite)
        {
            List<AdminVentaResumen> lista = new List<AdminVentaResumen>();

            using (SqlConnection con = new SqlConnection(conexion))
            {
                SqlCommand cmd = new SqlCommand(@"
                    SELECT TOP (@limite)
                        v.id_venta,
                        v.id_usuario,
                        LTRIM(RTRIM(ISNULL(u.nombre, '') + ' ' + ISNULL(u.apellido, ''))) AS cliente,
                        u.correo,
                        v.fecha_venta,
                        v.metodo_pago,
                        v.total,
                        v.estado,
                        ISNULL(SUM(dv.cantidad), 0) AS cantidad_productos
                    FROM ventas v
                    INNER JOIN usuarios u ON v.id_usuario = u.id_usuario
                    LEFT JOIN detalle_venta dv ON v.id_venta = dv.id_venta
                    GROUP BY
                        v.id_venta,
                        v.id_usuario,
                        u.nombre,
                        u.apellido,
                        u.correo,
                        v.fecha_venta,
                        v.metodo_pago,
                        v.total,
                        v.estado
                    ORDER BY v.fecha_venta DESC;
                ", con);

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@limite", SqlDbType.Int).Value = limite;

                con.Open();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        lista.Add(new AdminVentaResumen()
                        {
                            IdVenta = Convert.ToInt32(dr["id_venta"]),
                            IdUsuario = Convert.ToInt32(dr["id_usuario"]),
                            Cliente = dr["cliente"].ToString(),
                            Correo = dr["correo"].ToString(),
                            FechaVenta = Convert.ToDateTime(dr["fecha_venta"]),
                            MetodoPago = dr["metodo_pago"].ToString(),
                            Total = Convert.ToDecimal(dr["total"]),
                            Estado = dr["estado"].ToString(),
                            CantidadProductos = Convert.ToInt32(dr["cantidad_productos"])
                        });
                    }
                }
            }

            return lista;
        }

        public static List<AdminVentaDetalle> ObtenerDetalleVenta(int idVenta)
        {
            List<AdminVentaDetalle> lista = new List<AdminVentaDetalle>();

            using (SqlConnection con = new SqlConnection(conexion))
            {
                SqlCommand cmd = new SqlCommand(@"
                    SELECT
                        dv.id_detalle,
                        dv.id_venta,
                        p.nombre_producto AS producto,
                        dv.cantidad,
                        dv.precio_unitario,
                        dv.subtotal
                    FROM detalle_venta dv
                    INNER JOIN productos p ON dv.id_producto = p.id_producto
                    WHERE dv.id_venta = @id_venta
                    ORDER BY dv.id_detalle ASC;
                ", con);

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@id_venta", SqlDbType.Int).Value = idVenta;

                con.Open();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        lista.Add(new AdminVentaDetalle()
                        {
                            IdDetalle = Convert.ToInt32(dr["id_detalle"]),
                            IdVenta = Convert.ToInt32(dr["id_venta"]),
                            Producto = dr["producto"].ToString(),
                            Cantidad = Convert.ToInt32(dr["cantidad"]),
                            PrecioUnitario = Convert.ToDecimal(dr["precio_unitario"]),
                            Subtotal = Convert.ToDecimal(dr["subtotal"])
                        });
                    }
                }
            }

            return lista;
        }

        public static List<AdminGraficaVenta> ObtenerGraficaVentas(int? idUsuario)
        {
            List<AdminGraficaVenta> lista = new List<AdminGraficaVenta>();

            using (SqlConnection con = new SqlConnection(conexion))
            {
                SqlCommand cmd;

                if (idUsuario.HasValue)
                {
                    cmd = new SqlCommand(@"
                        SELECT
                            CONVERT(VARCHAR(10), CONVERT(DATE, v.fecha_venta), 120) AS etiqueta,
                            COUNT(v.id_venta) AS cantidad_ventas,
                            ISNULL(SUM(v.total), 0) AS total_ventas
                        FROM ventas v
                        WHERE v.id_usuario = @id_usuario
                        GROUP BY CONVERT(DATE, v.fecha_venta)
                        ORDER BY CONVERT(DATE, v.fecha_venta) ASC;
                    ", con);

                    cmd.Parameters.Add("@id_usuario", SqlDbType.Int).Value = idUsuario.Value;
                }
                else
                {
                    cmd = new SqlCommand(@"
                        SELECT
                            LTRIM(RTRIM(ISNULL(u.nombre, '') + ' ' + ISNULL(u.apellido, ''))) AS etiqueta,
                            COUNT(v.id_venta) AS cantidad_ventas,
                            ISNULL(SUM(v.total), 0) AS total_ventas
                        FROM usuarios u
                        LEFT JOIN ventas v ON u.id_usuario = v.id_usuario
                        GROUP BY
                            u.id_usuario,
                            u.nombre,
                            u.apellido
                        ORDER BY total_ventas DESC, cantidad_ventas DESC;
                    ", con);
                }

                cmd.CommandType = CommandType.Text;

                con.Open();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        lista.Add(new AdminGraficaVenta()
                        {
                            Etiqueta = dr["etiqueta"].ToString(),
                            CantidadVentas = Convert.ToInt32(dr["cantidad_ventas"]),
                            TotalVentas = Convert.ToDecimal(dr["total_ventas"])
                        });
                    }
                }
            }

            return lista;
        }

        public static List<AdminReporteVenta> ObtenerReporteVentas(DateTime desde, DateTime hastaExclusivo)
        {
            List<AdminReporteVenta> lista = new List<AdminReporteVenta>();

            using (SqlConnection con = new SqlConnection(conexion))
            {
                SqlCommand cmd = new SqlCommand(@"
                    SELECT
                        v.id_venta,
                        v.fecha_venta,
                        LTRIM(RTRIM(ISNULL(u.nombre, '') + ' ' + ISNULL(u.apellido, ''))) AS nombre_cliente,
                        u.correo AS correo_cliente,
                        v.metodo_pago,
                        v.estado,
                        p.nombre_producto,
                        dv.cantidad,
                        dv.precio_unitario,
                        dv.subtotal,
                        v.total AS total_venta
                    FROM ventas v
                    INNER JOIN usuarios u ON v.id_usuario = u.id_usuario
                    INNER JOIN detalle_venta dv ON v.id_venta = dv.id_venta
                    INNER JOIN productos p ON dv.id_producto = p.id_producto
                    WHERE v.fecha_venta >= @desde 
                    AND v.fecha_venta < @hasta
                    ORDER BY v.fecha_venta ASC, v.id_venta ASC;
                ", con);

                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = 120;

                cmd.Parameters.Add("@desde", SqlDbType.DateTime).Value = desde;
                cmd.Parameters.Add("@hasta", SqlDbType.DateTime).Value = hastaExclusivo;

                con.Open();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        lista.Add(new AdminReporteVenta()
                        {
                            IdVenta = Convert.ToInt32(dr["id_venta"]),
                            FechaVenta = Convert.ToDateTime(dr["fecha_venta"]),
                            NombreCliente = dr["nombre_cliente"].ToString(),
                            CorreoCliente = dr["correo_cliente"].ToString(),
                            MetodoPago = dr["metodo_pago"].ToString(),
                            Estado = dr["estado"].ToString(),
                            NombreProducto = dr["nombre_producto"].ToString(),
                            Cantidad = Convert.ToInt32(dr["cantidad"]),
                            PrecioUnitario = Convert.ToDecimal(dr["precio_unitario"]),
                            Subtotal = Convert.ToDecimal(dr["subtotal"]),
                            TotalVenta = Convert.ToDecimal(dr["total_venta"])
                        });
                    }
                }
            }

            return lista;
        }
    }
}