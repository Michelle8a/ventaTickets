using System.Web;
using System.Web.Mvc;

namespace GestionTickets
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            // Fuerza HTTPS en toda la aplicación.
            // En VS: Project Properties → Web → habilita "SSL Enabled" para que funcione en local.
            //filters.Add(new RequireHttpsAttribute());
        }
    }
}
