using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;

namespace GestionTickets.Controllers
{
    public class LikesController : Controller
    {
        private gestion_ticketsEntities db = new gestion_ticketsEntities();

        // GET: Likes
        public ActionResult Index(int? id_usuario)
        {
            if (id_usuario == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var likes = db.sp_mostrar_likes(id_usuario, null).ToList();
            ViewBag.id_usuario = id_usuario;
            return View(likes);
        }


        // POST: Likes/Toggle
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Toggle(int id_usuario, int id_presentacion, string returnUrl)
        {
            if (id_usuario <= 0 || id_presentacion <= 0)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var likeExistente = db.likes.FirstOrDefault(l =>
                l.id_usuario == id_usuario &&
                l.id_presentacion == id_presentacion &&
                l.activo == true);

            try
            {
                if (likeExistente != null)
                    db.sp_quitar_like(id_usuario, id_presentacion);
                else
                    db.sp_agregar_like(id_usuario, id_presentacion);
            }
            catch (Exception ex)
            {
                // Si hay conflicto de horario el SP lanza un error, lo mostramos
                TempData["Error"] = ex.Message;
            }

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", new { id_usuario });
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