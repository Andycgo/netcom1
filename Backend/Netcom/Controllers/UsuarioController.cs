using Netcom.Data;
using Netcom.Models;
using System;
using System.Collections.Generic;
using System.Web.Http;

namespace Netcom.Controllers
{
    [RoutePrefix("api/usuario")]
    public class UsuarioController : ApiController
    {
       
        // GET: api/usuario
        [HttpGet]
        [Route("")]
        public IHttpActionResult Get()
        {
            List<Usuario> lista = UsuarioData.ListarUsuarios();
            return Ok(lista);
        }

        // GET: api/usuario/5
        [HttpGet]
        [Route("{id:int}")]
        public IHttpActionResult Get(int id)
        {
            Usuario usuario = UsuarioData.ConsultarUsuario(id);

            if (usuario == null)
                return NotFound();

            return Ok(usuario);
        }

        // POST: api/usuario
        [HttpPost]
        [Route("")]
        public IHttpActionResult Post([FromBody] Usuario oUsuario)
        {
            if (oUsuario == null)
                return BadRequest("Los datos del usuario son obligatorios.");

            bool respuesta = UsuarioData.RegistrarUsuario(oUsuario);

            if (respuesta)
                return Ok("Usuario registrado correctamente.");

            return BadRequest("No se pudo registrar el usuario.");
        }

        // PUT: api/usuario
        [HttpPut]
        [Route("")]
        public IHttpActionResult Put([FromBody] Usuario oUsuario)
        {
            if (oUsuario == null)
                return BadRequest("Los datos del usuario son obligatorios.");

            bool respuesta = UsuarioData.ActualizarUsuario(oUsuario);

            if (respuesta)
                return Ok("Usuario actualizado correctamente.");

            return BadRequest("No se pudo actualizar el usuario.");
        }

        // DELETE: api/usuario/5
        [HttpDelete]
        [Route("{id:int}")]
        public IHttpActionResult Delete(int id)
        {
            bool respuesta = UsuarioData.EliminarUsuario(id);

            if (respuesta)
                return Ok("Usuario eliminado correctamente.");

            return BadRequest("No se pudo eliminar el usuario.");
        }

        // POST: api/usuario/login
        [HttpPost]
        [Route("login")]
        public IHttpActionResult Login([FromBody] Usuario oUsuario)
        {
            if (oUsuario == null)
                return BadRequest("Los datos de inicio de sesión son obligatorios.");

            Usuario usuario = UsuarioData.LoginUsuario(
                oUsuario.Correo,
                oUsuario.Contrasena
            );

            if (usuario == null)
                return Unauthorized();

            return Ok(usuario);
        }
    }
}