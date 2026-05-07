using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;

namespace GestionTickets.Controllers
{
    public class AgendaController : Controller
    {
        private gestion_ticketsEntities db = new gestion_ticketsEntities();

        // GET: Agenda
        public ActionResult Index()
        {
            var usuarioSesion = Session["usuario"] as usuarios;

            if (usuarioSesion == null)
                return RedirectToAction("Login", "Acceso");

            var agenda = db.sp_mostrar_agenda(usuarioSesion.id_usuario).ToList();
            ViewBag.id_usuario = usuarioSesion.id_usuario;
            return View(agenda);
        }

        // POST: Agenda/ActualizarRecordatorio
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ActualizarRecordatorio(int id_agenda, bool recordatorio, string returnUrl = null)
        {
            var usuarioSesion = Session["usuario"] as usuarios;

            if (usuarioSesion == null)
                return RedirectToAction("Login", "Acceso");

            var item = db.agenda_usuario.Find(id_agenda);

            if (item == null)
                return HttpNotFound();

            db.sp_actualizar_agenda(id_agenda, recordatorio);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index");
        }

        // METODO PARA LIBERAR RECURSOS
        protected override void Dispose(bool disposing)
        {
            if (disposing)
                db.Dispose();

            base.Dispose(disposing);
        }
    }
}