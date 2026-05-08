using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace GestionTickets.Permisos
{
    public class ValidarRolAttribute : ActionFilterAttribute
    {
        private readonly string[] rolesPermitidos;

        public ValidarRolAttribute(params string[] roles)
        {
            rolesPermitidos = roles;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var rol = HttpContext.Current.Session["rol"] as string;

            if (string.IsNullOrEmpty(rol) || !rolesPermitidos.Contains(rol))
            {
                filterContext.Result = new RedirectResult("~/Home/Index");
            }

            base.OnActionExecuting(filterContext);
        }
    }
}