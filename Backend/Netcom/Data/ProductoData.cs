using Netcom.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Netcom.Data
{
    public class ProductoData
    {
        private static string conexion =
            @"Data Source=MILLER\SQLEXPRESS;Initial Catalog=netcom;Integrated Security=True";

        private static bool ExisteColumna(string tabla, string columna)
        {
            using (SqlConnection con = new SqlConnection(conexion))
            {
                SqlCommand cmd = new SqlCommand(@"
                    SELECT COUNT(*)
                    FROM INFORMATION_SCHEMA.COLUMNS
                    WHERE TABLE_NAME = @tabla
                    AND COLUMN_NAME = @columna;
                ", con);

                cmd.Parameters.Add("@tabla", SqlDbType.VarChar, 100).Value = tabla;
                cmd.Parameters.Add("@columna", SqlDbType.VarChar, 100).Value = columna;

                con.Open();

                int cantidad = Convert.ToInt32(cmd.ExecuteScalar());

                return cantidad > 0;
            }
        }

        private static string ObtenerCampoImagen()
        {
            if (ExisteColumna("productos", "imagen"))
                return "imagen";

            if (ExisteColumna("productos", "ruta_imagen"))
                return "ruta_imagen";

            return "";
        }

        private static Producto MapearProducto(SqlDataReader dr)
        {
            return new Producto()
            {
                IdProducto = Convert.ToInt32(dr["id_producto"]),
                NombreProducto = dr["nombre_producto"].ToString(),
                Descripcion = dr["descripcion"].ToString(),
                Precio = Convert.ToDecimal(dr["precio"]),
                Stock = Convert.ToInt32(dr["stock"]),
                Imagen = dr["imagen"] == DBNull.Value ? "" : dr["imagen"].ToString(),
                Marca = dr["marca"].ToString(),
                EstadoProducto = dr["estado_producto"].ToString(),
                FechaPublicacion = dr["fecha_publicacion"] == DBNull.Value
                    ? DateTime.Now
                    : Convert.ToDateTime(dr["fecha_publicacion"]),
                IdCategoria = Convert.ToInt32(dr["id_categoria"]),
                NombreCategoria = dr["nombre_categoria"].ToString()
            };
        }

        public static List<Producto> ListarProductos()
        {
            List<Producto> lista = new List<Producto>();

            string campoImagen = ObtenerCampoImagen();

            string imagenSql = string.IsNullOrEmpty(campoImagen)
                ? "'' AS imagen"
                : "ISNULL(p." + campoImagen + ", '') AS imagen";

            string estadoSql = ExisteColumna("productos", "estado_producto")
                ? "ISNULL(p.estado_producto, CASE WHEN p.stock <= 0 THEN 'agotado' ELSE 'disponible' END) AS estado_producto"
                : "CASE WHEN p.stock <= 0 THEN 'agotado' ELSE 'disponible' END AS estado_producto";

            string fechaSql = ExisteColumna("productos", "fecha_publicacion")
                ? "ISNULL(p.fecha_publicacion, GETDATE()) AS fecha_publicacion"
                : "GETDATE() AS fecha_publicacion";

            using (SqlConnection con = new SqlConnection(conexion))
            {
                SqlCommand cmd = new SqlCommand(@"
                    SELECT
                        p.id_producto,
                        p.nombre_producto,
                        p.descripcion,
                        p.precio,
                        p.stock,
                        " + imagenSql + @",
                        p.marca,
                        " + estadoSql + @",
                        " + fechaSql + @",
                        p.id_categoria,
                        ISNULL(c.nombre_categoria, 'Sin categoría') AS nombre_categoria
                    FROM productos p
                    LEFT JOIN categorias c ON p.id_categoria = c.id_categoria
                    WHERE 
                        " + (ExisteColumna("productos", "estado_producto")
                            ? "ISNULL(p.estado_producto, '') <> 'eliminado'"
                            : "1 = 1") + @"
                    ORDER BY p.id_producto DESC;
                ", con);

                cmd.CommandType = CommandType.Text;

                con.Open();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        lista.Add(MapearProducto(dr));
                    }
                }
            }

            return lista;
        }

        public static Producto ConsultarProducto(int id)
        {
            Producto producto = null;

            string campoImagen = ObtenerCampoImagen();

            string imagenSql = string.IsNullOrEmpty(campoImagen)
                ? "'' AS imagen"
                : "ISNULL(p." + campoImagen + ", '') AS imagen";

            string estadoSql = ExisteColumna("productos", "estado_producto")
                ? "ISNULL(p.estado_producto, CASE WHEN p.stock <= 0 THEN 'agotado' ELSE 'disponible' END) AS estado_producto"
                : "CASE WHEN p.stock <= 0 THEN 'agotado' ELSE 'disponible' END AS estado_producto";

            string fechaSql = ExisteColumna("productos", "fecha_publicacion")
                ? "ISNULL(p.fecha_publicacion, GETDATE()) AS fecha_publicacion"
                : "GETDATE() AS fecha_publicacion";

            using (SqlConnection con = new SqlConnection(conexion))
            {
                SqlCommand cmd = new SqlCommand(@"
                    SELECT
                        p.id_producto,
                        p.nombre_producto,
                        p.descripcion,
                        p.precio,
                        p.stock,
                        " + imagenSql + @",
                        p.marca,
                        " + estadoSql + @",
                        " + fechaSql + @",
                        p.id_categoria,
                        ISNULL(c.nombre_categoria, 'Sin categoría') AS nombre_categoria
                    FROM productos p
                    LEFT JOIN categorias c ON p.id_categoria = c.id_categoria
                    WHERE p.id_producto = @id_producto;
                ", con);

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@id_producto", SqlDbType.Int).Value = id;

                con.Open();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        producto = MapearProducto(dr);
                    }
                }
            }

            return producto;
        }

        public static bool InsertarProducto(Producto producto, out string mensaje)
        {
            mensaje = "";

            string campoImagen = ObtenerCampoImagen();
            bool tieneEstado = ExisteColumna("productos", "estado_producto");
            bool tieneFecha = ExisteColumna("productos", "fecha_publicacion");

            using (SqlConnection con = new SqlConnection(conexion))
            {
                string columnas = @"
                    nombre_producto,
                    descripcion,
                    precio,
                    stock,
                    marca,
                    id_categoria";

                string valores = @"
                    @nombre_producto,
                    @descripcion,
                    @precio,
                    @stock,
                    @marca,
                    @id_categoria";

                if (!string.IsNullOrEmpty(campoImagen))
                {
                    columnas += ", " + campoImagen;
                    valores += ", @imagen";
                }

                if (tieneEstado)
                {
                    columnas += ", estado_producto";
                    valores += ", @estado_producto";
                }

                if (tieneFecha)
                {
                    columnas += ", fecha_publicacion";
                    valores += ", GETDATE()";
                }

                SqlCommand cmd = new SqlCommand(@"
                    INSERT INTO productos
                    (
                        " + columnas + @"
                    )
                    VALUES
                    (
                        " + valores + @"
                    );
                ", con);

                cmd.CommandType = CommandType.Text;

                cmd.Parameters.Add("@nombre_producto", SqlDbType.VarChar, 150).Value = producto.NombreProducto;
                cmd.Parameters.Add("@descripcion", SqlDbType.VarChar, 500).Value = producto.Descripcion;
                SqlParameter precioParam = cmd.Parameters.Add("@precio", SqlDbType.Decimal);
                precioParam.Precision = 18;
                precioParam.Scale = 2;
                precioParam.Value = producto.Precio; cmd.Parameters.Add("@marca", SqlDbType.VarChar, 100).Value = producto.Marca;
                cmd.Parameters.Add("@id_categoria", SqlDbType.Int).Value = producto.IdCategoria;

                if (!string.IsNullOrEmpty(campoImagen))
                {
                    cmd.Parameters.Add("@imagen", SqlDbType.VarChar, 255).Value =
                        string.IsNullOrWhiteSpace(producto.Imagen)
                            ? (object)DBNull.Value
                            : producto.Imagen;
                }

                if (tieneEstado)
                {
                    cmd.Parameters.Add("@estado_producto", SqlDbType.VarChar, 30).Value =
                        producto.Stock <= 0 ? "agotado" : "disponible";
                }

                con.Open();

                int filas = cmd.ExecuteNonQuery();

                if (filas > 0)
                {
                    mensaje = "Producto registrado correctamente.";
                    return true;
                }

                mensaje = "No se pudo registrar el producto.";
                return false;
            }
        }

        public static bool ActualizarProducto(Producto producto, out string mensaje)
        {
            mensaje = "";

            string campoImagen = ObtenerCampoImagen();
            bool tieneEstado = ExisteColumna("productos", "estado_producto");

            using (SqlConnection con = new SqlConnection(conexion))
            {
                string setImagen = "";

                if (!string.IsNullOrEmpty(campoImagen))
                {
                    setImagen = ", " + campoImagen + " = @imagen";
                }

                string setEstado = "";

                if (tieneEstado)
                {
                    setEstado = @",
                        estado_producto = CASE 
                            WHEN @stock <= 0 THEN 'agotado'
                            ELSE 'disponible'
                        END";
                }

                SqlCommand cmd = new SqlCommand(@"
                    UPDATE productos
                    SET
                        nombre_producto = @nombre_producto,
                        descripcion = @descripcion,
                        precio = @precio,
                        stock = @stock,
                        marca = @marca,
                        id_categoria = @id_categoria
                        " + setImagen + @"
                        " + setEstado + @"
                    WHERE id_producto = @id_producto;
                ", con);

                cmd.CommandType = CommandType.Text;

                cmd.Parameters.Add("@id_producto", SqlDbType.Int).Value = producto.IdProducto;
                cmd.Parameters.Add("@nombre_producto", SqlDbType.VarChar, 150).Value = producto.NombreProducto;
                cmd.Parameters.Add("@descripcion", SqlDbType.VarChar, 500).Value = producto.Descripcion;
SqlParameter precioParam = cmd.Parameters.Add("@precio", SqlDbType.Decimal);
precioParam.Precision = 18;
precioParam.Scale = 2;
precioParam.Value = producto.Precio;                cmd.Parameters.Add("@stock", SqlDbType.Int).Value = producto.Stock;
                cmd.Parameters.Add("@marca", SqlDbType.VarChar, 100).Value = producto.Marca;
                cmd.Parameters.Add("@id_categoria", SqlDbType.Int).Value = producto.IdCategoria;

                if (!string.IsNullOrEmpty(campoImagen))
                {
                    cmd.Parameters.Add("@imagen", SqlDbType.VarChar, 255).Value =
                        string.IsNullOrWhiteSpace(producto.Imagen)
                            ? (object)DBNull.Value
                            : producto.Imagen;
                }

                con.Open();

                int filas = cmd.ExecuteNonQuery();

                if (filas > 0)
                {
                    mensaje = "Producto actualizado correctamente.";
                    return true;
                }

                mensaje = "No se encontró el producto para actualizar.";
                return false;
            }
        }

        public static bool EliminarProducto(int id, out string mensaje)
        {
            mensaje = "";

            bool tieneEstado = ExisteColumna("productos", "estado_producto");

            using (SqlConnection con = new SqlConnection(conexion))
            {
                con.Open();

                try
                {
                    SqlCommand cmdDelete = new SqlCommand(@"
                        DELETE FROM productos
                        WHERE id_producto = @id_producto;
                    ", con);

                    cmdDelete.CommandType = CommandType.Text;
                    cmdDelete.Parameters.Add("@id_producto", SqlDbType.Int).Value = id;

                    int filas = cmdDelete.ExecuteNonQuery();

                    if (filas > 0)
                    {
                        mensaje = "Producto eliminado correctamente.";
                        return true;
                    }

                    mensaje = "No se encontró el producto para eliminar.";
                    return false;
                }
                catch
                {
                    if (!tieneEstado)
                    {
                        mensaje = "No se puede eliminar el producto porque ya tiene ventas relacionadas.";
                        return false;
                    }

                    SqlCommand cmdSoftDelete = new SqlCommand(@"
                        UPDATE productos
                        SET 
                            estado_producto = 'eliminado',
                            stock = 0
                        WHERE id_producto = @id_producto;
                    ", con);

                    cmdSoftDelete.CommandType = CommandType.Text;
                    cmdSoftDelete.Parameters.Add("@id_producto", SqlDbType.Int).Value = id;

                    int filasSoft = cmdSoftDelete.ExecuteNonQuery();

                    if (filasSoft > 0)
                    {
                        mensaje = "Producto eliminado correctamente.";
                        return true;
                    }

                    mensaje = "No se encontró el producto para eliminar.";
                    return false;
                }
            }
        }
    }
}