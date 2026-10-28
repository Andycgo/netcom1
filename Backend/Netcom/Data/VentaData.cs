using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Netcom.Models;
using System.Data;
using System.Data.SqlClient;

namespace Netcom.Data
{
    public class VentaData
    {
        private static string conexion =
            @"Data Source=MILLER\SQLEXPRESS;Initial Catalog=netcom;Integrated Security=True";

        public static int RegistrarVenta(VentaRequest venta, out string mensaje)
        {
            mensaje = "";

            if (venta == null)
            {
                mensaje = "Los datos de la venta son obligatorios.";
                return 0;
            }

            if (venta.IdUsuario <= 0)
            {
                mensaje = "El usuario no es válido.";
                return 0;
            }

            if (venta.Detalles == null || venta.Detalles.Count == 0)
            {
                mensaje = "El carrito está vacío.";
                return 0;
            }

            using (SqlConnection con = new SqlConnection(conexion))
            {
                con.Open();

                SqlTransaction transaccion = con.BeginTransaction();

                try
                {
                    decimal totalVenta = 0;

                    SqlCommand cmdVenta = new SqlCommand(@"
    INSERT INTO ventas 
    (
        id_usuario, 
        fecha_venta, 
        metodo_pago, 
        total, 
        estado
    )
    OUTPUT INSERTED.id_venta
    VALUES
    (
        @id_usuario, 
        GETDATE(), 
        @metodo_pago, 
        0, 
        @estado
    );
", con, transaccion);

                    cmdVenta.Parameters.Add("@id_usuario", SqlDbType.Int).Value = venta.IdUsuario;

                    cmdVenta.Parameters.Add("@metodo_pago", SqlDbType.VarChar, 50).Value =
                        string.IsNullOrWhiteSpace(venta.MetodoPago)
                            ? "No especificado"
                            : venta.MetodoPago;

                    cmdVenta.Parameters.Add("@estado", SqlDbType.VarChar, 30).Value = "realizada";

                    int idVenta = Convert.ToInt32(cmdVenta.ExecuteScalar());

                    foreach (VentaDetalle item in venta.Detalles)
                    {
                        if (item.IdProducto <= 0)
                        {
                            throw new Exception("Uno de los productos no es válido.");
                        }

                        if (item.Cantidad <= 0)
                        {
                            throw new Exception("La cantidad debe ser mayor que cero.");
                        }

                        SqlCommand cmdProducto = new SqlCommand(@"
                            SELECT nombre_producto, precio, stock
                            FROM productos
                            WHERE id_producto = @id_producto;
                        ", con, transaccion);

                        cmdProducto.Parameters.Add("@id_producto", SqlDbType.Int).Value = item.IdProducto;

                        string nombreProducto = "";
                        decimal precioUnitario = 0;
                        int stockActual = 0;

                        using (SqlDataReader dr = cmdProducto.ExecuteReader())
                        {
                            if (!dr.Read())
                            {
                                throw new Exception("El producto con ID " + item.IdProducto + " no existe.");
                            }

                            nombreProducto = dr["nombre_producto"].ToString();
                            precioUnitario = Convert.ToDecimal(dr["precio"]);
                            stockActual = Convert.ToInt32(dr["stock"]);
                        }

                        if (stockActual <= 0)
                        {
                            throw new Exception("El producto " + nombreProducto + " está agotado.");
                        }

                        if (stockActual < item.Cantidad)
                        {
                            throw new Exception("No hay stock suficiente para " + nombreProducto + ".");
                        }

                        decimal subtotal = precioUnitario * item.Cantidad;
                        totalVenta += subtotal;

                        SqlCommand cmdDetalle = new SqlCommand(@"
                            INSERT INTO detalle_venta(id_venta, id_producto, cantidad, precio_unitario, subtotal)
                            VALUES(@id_venta, @id_producto, @cantidad, @precio_unitario, @subtotal);
                        ", con, transaccion);

                        cmdDetalle.Parameters.Add("@id_venta", SqlDbType.Int).Value = idVenta;
                        cmdDetalle.Parameters.Add("@id_producto", SqlDbType.Int).Value = item.IdProducto;
                        cmdDetalle.Parameters.Add("@cantidad", SqlDbType.Int).Value = item.Cantidad;
                        cmdDetalle.Parameters.Add("@precio_unitario", SqlDbType.Decimal).Value = precioUnitario;
                        cmdDetalle.Parameters.Add("@subtotal", SqlDbType.Decimal).Value = subtotal;

                        cmdDetalle.ExecuteNonQuery();

                        SqlCommand cmdStock = new SqlCommand(@"
                            UPDATE productos
                            SET 
                                stock = stock - @cantidad,
                                estado_producto = CASE 
                                    WHEN stock - @cantidad <= 0 THEN 'agotado'
                                    ELSE 'disponible'
                                END
                            WHERE id_producto = @id_producto
                            AND stock >= @cantidad;
                        ", con, transaccion);

                        cmdStock.Parameters.Add("@cantidad", SqlDbType.Int).Value = item.Cantidad;
                        cmdStock.Parameters.Add("@id_producto", SqlDbType.Int).Value = item.IdProducto;

                        int filasActualizadas = cmdStock.ExecuteNonQuery();

                        if (filasActualizadas == 0)
                        {
                            throw new Exception("No se pudo descontar el stock de " + nombreProducto + ".");
                        }
                    }

                    SqlCommand cmdActualizarVenta = new SqlCommand(@"
                        UPDATE ventas
                        SET total = @total
                        WHERE id_venta = @id_venta;
                    ", con, transaccion);

                    cmdActualizarVenta.Parameters.Add("@total", SqlDbType.Decimal).Value = totalVenta;
                    cmdActualizarVenta.Parameters.Add("@id_venta", SqlDbType.Int).Value = idVenta;

                    cmdActualizarVenta.ExecuteNonQuery();

                    transaccion.Commit();

                    mensaje = "Venta registrada correctamente.";
                    return idVenta;
                }
                catch (Exception ex)
                {
                    transaccion.Rollback();
                    mensaje = ex.Message;
                    return 0;
                }
            }
        }

        public static VentaResumen ObtenerResumenUsuario(int idUsuario)
        {
            VentaResumen resumen = null;

            using (SqlConnection con = new SqlConnection(conexion))
            {
                SqlCommand cmd = new SqlCommand(@"
                    SELECT
                        u.id_usuario,
                        u.nombre + ' ' + u.apellido AS nombre_completo,
                        u.correo,
                        u.rol,
                        u.fecha_registro,
                        COUNT(v.id_venta) AS compras_realizadas,
                        ISNULL(SUM(v.total), 0) AS total_comprado,
                        MAX(v.fecha_venta) AS ultima_compra
                    FROM usuarios u
                    LEFT JOIN ventas v ON u.id_usuario = v.id_usuario
                    WHERE u.id_usuario = @id_usuario
                    GROUP BY
                        u.id_usuario,
                        u.nombre,
                        u.apellido,
                        u.correo,
                        u.rol,
                        u.fecha_registro;
                ", con);

                cmd.Parameters.Add("@id_usuario", SqlDbType.Int).Value = idUsuario;

                try
                {
                    con.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            resumen = new VentaResumen()
                            {
                                IdUsuario = Convert.ToInt32(dr["id_usuario"]),
                                NombreCompleto = dr["nombre_completo"].ToString(),
                                Correo = dr["correo"].ToString(),
                                Rol = dr["rol"].ToString(),
                                FechaRegistro = Convert.ToDateTime(dr["fecha_registro"]),
                                ComprasRealizadas = Convert.ToInt32(dr["compras_realizadas"]),
                                TotalComprado = Convert.ToDecimal(dr["total_comprado"]),
                                UltimaCompra = dr["ultima_compra"] == DBNull.Value
                                    ? (DateTime?)null
                                    : Convert.ToDateTime(dr["ultima_compra"])
                            };
                        }
                    }
                }
                catch
                {
                    resumen = null;
                }
            }

            return resumen;
        }
    }
}