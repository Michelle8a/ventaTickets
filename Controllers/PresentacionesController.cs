using GestionTickets.Permisos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace GestionTickets.Controllers
{
    public class PresentacionesController : Controller
    {
        private gestion_ticketsEntities db = new gestion_ticketsEntities();

        // GET: Presentaciones
        public ActionResult Index()
        {
            var usuarioSesion = Session["usuario"] as usuarios;

            var presentaciones = db.sp_mostrar_presentaciones(null).ToList();

            if (usuarioSesion != null)
            {
                ViewBag.id_usuario = usuarioSesion.id_usuario;

                // Lista de id_presentacion que el usuario ya tiene en likes
                var likes = db.likes
                    .Where(l => l.id_usuario == usuarioSesion.id_usuario && l.activo == true)
                    .Select(l => l.id_presentacion)
                    .ToList();

                ViewBag.Likes = likes;
            }
            else
            {
                ViewBag.id_usuario = 0;
                ViewBag.Likes = new List<int?>();
            }

            return View(presentaciones);
        }

        // GET: Presentaciones/Crear
        [ValidarRol("Admin", "Organizador")]
        public ActionResult Crear()
        {
            ViewBag.Eventos = db.sp_mostrar_eventos(null).ToList();
            ViewBag.Artistas = db.sp_mostrar_artistas(null).ToList();
            return View();
        }

        // POST: Presentaciones/Crear
        [HttpPost]
        [ValidarRol("Admin", "Organizador")]
        public ActionResult Crear(int id_evento, int id_artista, DateTime fecha_hora_inicio,
            string escenario, DateTime? fecha_hora_fin, int? orden)
        {
            db.sp_agregar_presentacion(id_evento, id_artista, fecha_hora_inicio,
                escenario, fecha_hora_fin, orden);
            return RedirectToAction("Index");
        }

        // GET: Presentaciones/Editar/5
        [ValidarRol("Admin", "Organizador")]
        public ActionResult Editar(int id)
        {
            var presentacion = db.sp_mostrar_presentaciones(null)
                .FirstOrDefault(p => p.id_presentacion == id);

            if (presentacion == null)
                return HttpNotFound();

            ViewBag.Artistas = db.sp_mostrar_artistas(null).ToList();
            return View(presentacion);
        }

        // POST: Presentaciones/Editar/5
        [HttpPost]
        [ValidarRol("Admin", "Organizador")]
        public ActionResult Editar(int id_presentacion, int id_artista, string escenario,
            DateTime? fecha_hora_inicio, DateTime? fecha_hora_fin, int? orden)
        {
            db.sp_actualizar_presentacion(id_presentacion, id_artista, escenario,
                fecha_hora_inicio, fecha_hora_fin, orden);
            return RedirectToAction("Index");
        }

        // POST: Presentaciones/Eliminar/5
        [HttpPost]
        [ValidarRol("Admin", "Organizador")]
        public ActionResult Eliminar(int id)
        {
            db.sp_eliminar_presentacion(id);
            return RedirectToAction("Index");
        }

        // POST: Presentaciones/Activar/5
        [HttpPost]
        [ValidarRol("Admin", "Organizador")]
        public ActionResult Activar(int id)
        {
            db.sp_activar_presentacion(id);
            return RedirectToAction("Index");
        }
    }
}