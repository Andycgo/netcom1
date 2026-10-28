using Netcom.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Data;

namespace Netcom.Data
{
    public class UsuarioData
    {
        private static string conexion =
    @"Data Source=MILLER\SQLEXPRESS;Initial Catalog=netcom;Integrated Security=True";

        public static object ProbarConexionUsuarios()
        {
            using (SqlConnection con = new SqlConnection(conexion))
            {
                SqlCommand cmd = new SqlCommand(@"
            SELECT 
                @@SERVERNAME AS servidor,
                DB_NAME() AS base_datos,
                COUNT(*) AS cantidad_usuarios
            FROM usuarios;
        ", con);

                con.Open();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        return new
                        {
                            servidor = dr["servidor"].ToString(),
                            baseDatos = dr["base_datos"].ToString(),
                            cantidadUsuarios = Convert.ToInt32(dr["cantidad_usuarios"])
                        };
                    }
                }
            }

            return new
            {
                mensaje = "No se pudo leer la información de conexión."
            };
        }

        // ======================================================
        // REGISTRAR USUARIO
        // ======================================================

        public static bool RegistrarUsuario(Usuario oUsuario)
    {
        bool respuesta = false;

        using (SqlConnection con = new SqlConnection(conexion))
        {
            SqlCommand cmd = new SqlCommand(
                "registrar_usuario",
                con
            );

            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@nombre", oUsuario.Nombre);
            cmd.Parameters.AddWithValue("@apellido", oUsuario.Apellido);
            cmd.Parameters.AddWithValue("@correo", oUsuario.Correo);
            cmd.Parameters.AddWithValue("@contrasena", oUsuario.Contrasena);
            cmd.Parameters.AddWithValue("@telefono", oUsuario.Telefono);
            cmd.Parameters.AddWithValue("@direccion", oUsuario.Direccion);

            try
            {
                con.Open();
                cmd.ExecuteNonQuery();
                respuesta = true;
            }
            catch (Exception)
            {
                respuesta = false;
            }
        }

        return respuesta;
    }
        // ======================================================
        // ACTUALIZAR USUARIO
        // ======================================================
        public static Usuario LoginUsuario(string correo, string contrasena) 
        {

            Usuario usuario = null;

            using (SqlConnection con = new SqlConnection(conexion))
            {
                SqlCommand cmd = new SqlCommand(
                    "login_usuario",
                    con
                );

                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@correo", correo);

                cmd.Parameters.AddWithValue("@contrasena", contrasena);

                try
                {
                    con.Open();

                    SqlDataReader dr = cmd.ExecuteReader();

                    if (dr.Read())
                    {
                        usuario = new Usuario()
                        {
                            IdUsuario = Convert.ToInt32(dr["id_usuario"]),
                            Nombre = dr["nombre"].ToString(),
                            Apellido = dr["apellido"].ToString(),
                            Correo = dr["correo"].ToString(),
                            Telefono = dr["telefono"].ToString(),
                            Direccion = dr["direccion"].ToString(),
                            Rol = dr["rol"].ToString(),
                            FechaRegistro = Convert.ToDateTime(dr["fecha_registro"])
                        };
                    }
                }
                catch (Exception)
                {
                    usuario = null;
                }
            }

            return usuario;
        }
        // ======================================================
        // ACTUALIZAR USUARIO
        // ======================================================

        public static bool ActualizarUsuario(Usuario oUsuario)
    {
        bool respuesta = false;

        using (SqlConnection con = new SqlConnection(conexion))
        {
            SqlCommand cmd = new SqlCommand(
                "actualizar_usuario",
                con
            );

            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@id_usuario", oUsuario.IdUsuario);
            cmd.Parameters.AddWithValue("@nombre", oUsuario.Nombre);
            cmd.Parameters.AddWithValue("@apellido", oUsuario.Apellido);
            cmd.Parameters.AddWithValue("@correo", oUsuario.Correo);
            cmd.Parameters.AddWithValue("@telefono", oUsuario.Telefono);
            cmd.Parameters.AddWithValue("@direccion", oUsuario.Direccion);

            try
            {
                con.Open();
                cmd.ExecuteNonQuery();
                respuesta = true;
            }
            catch (Exception)
            {
                respuesta = false;
            }
        }

        return respuesta;
    }

    // ======================================================
    // ELIMINAR USUARIO
    // ======================================================

    public static bool EliminarUsuario(int id)
    {
        bool respuesta = false;

        using (SqlConnection con = new SqlConnection(conexion))
        {
            SqlCommand cmd = new SqlCommand(
                "eliminar_usuario",
                con
            );

            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@id_usuario", id);

            try
            {
                con.Open();
                cmd.ExecuteNonQuery();
                respuesta = true;
            }
            catch (Exception)
            {
                respuesta = false;
            }
        }

        return respuesta;
    }

    // ======================================================
    // LISTAR USUARIOS
    // ======================================================

    public static List<Usuario> ListarUsuarios()
    {
        List<Usuario> lista = new List<Usuario>();

        using (SqlConnection con = new SqlConnection(conexion))
        {
            SqlCommand cmd = new SqlCommand(
                "listar_usuarios",
                con
            );

            cmd.CommandType = CommandType.StoredProcedure;

            try
            {
                con.Open();

                SqlDataReader dr = cmd.ExecuteReader();

                while (dr.Read())
                {
                    lista.Add(new Usuario()
                    {
                        IdUsuario = Convert.ToInt32(dr["id_usuario"]),
                        Nombre = dr["nombre"].ToString(),
                        Apellido = dr["apellido"].ToString(),
                        Correo = dr["correo"].ToString(),
                        Telefono = dr["telefono"].ToString(),
                        Direccion = dr["direccion"].ToString(),
                        Rol = dr["rol"].ToString()
                    });
                }
            }
               catch(Exception ex)
{
                    throw new Exception("Error al ejecutar listar_usuarios: " + ex.Message);
                }
            }

        return lista;
    }

    // ======================================================
    // CONSULTAR USUARIO
    // ======================================================

    public static Usuario ConsultarUsuario(int id)
    {
        Usuario usuario = null;

        using (SqlConnection con = new SqlConnection(conexion))
        {
            SqlCommand cmd = new SqlCommand(
                "consultar_usuario",
                con
            );

            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@id_usuario", id);

            try
            {
                con.Open();

                SqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    usuario = new Usuario()
                    {
                        IdUsuario = Convert.ToInt32(dr["id_usuario"]),
                        Nombre = dr["nombre"].ToString(),
                        Apellido = dr["apellido"].ToString(),
                        Correo = dr["correo"].ToString(),
                        Telefono = dr["telefono"].ToString(),
                        Direccion = dr["direccion"].ToString(),
                        Rol = dr["rol"].ToString()
                    };
                }
            }
            catch (Exception)
            {
                usuario = null;
            }
        }

        return usuario;
    }
    }
}