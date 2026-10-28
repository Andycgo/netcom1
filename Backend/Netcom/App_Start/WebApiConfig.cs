using System.Web.Http;
using System.Web.Http.Cors;

namespace Netcom
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Habilitar CORS para permitir peticiones desde el frontend
            var cors = new EnableCorsAttribute("*", "*", "*");
            config.EnableCors(cors);

            // Rutas por atributos: [RoutePrefix] y [Route]
            config.MapHttpAttributeRoutes();

            // Ruta por defecto
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}