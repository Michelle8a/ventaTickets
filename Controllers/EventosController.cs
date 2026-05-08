using GestionTickets.Permisos;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace GestionTickets.Controllers
{
    public class EventosController : Controller
    {
        private gestion_ticketsEntities db = new gestion_ticketsEntities();
        
        // GET: Eventos
        public ActionResult Index()
        {
            var eventos = db.sp_mostrar_eventos(null).ToList();
            return View(eventos);
        }

        // GET: Eventos/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var evento = db.eventos
                .Include(e => e.venues)
                .Include(e => e.categorias)
                .Include(e => e.usuarios)
                .FirstOrDefault(e => e.id_evento == id);

            if (evento == null)
                return HttpNotFound();

            return View(evento);
        }

        // GET: Eventos/Create
        [ValidarRol("Admin", "Organizador")]
        public ActionResult Create()
        {
            CargarCombos();
            return View();
        }

        //POST: Eventos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidarRol("Admin", "Organizador")]
        public ActionResult Create([Bind(Include = "id_venue,id_categoria,id_usuario,nombre,tipo,descripcion,imagen_url,fecha_inicio,fecha_fin,activo")] eventos evento)
        {
            ValidarEvento(evento);

            if (ModelState.IsValid)
            {
                evento.fecha_creacion = DateTime.Now;
                if (evento.activo == null)
                    evento.activo = true;

                db.eventos.Add(evento);
                db.SaveChanges();

                return RedirectToAction("Index");
            }
            CargarCombos(evento);
            return View(evento);
        }

        // GET: Eventos/Edit/5
        [ValidarRol("Admin", "Organizador")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            eventos evento = db.eventos.Find(id);

            if(evento == null)
                return HttpNotFound();

            CargarCombos(evento);
            return View(evento);
        }

        // POST: Eventos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidarRol("Admin", "Organizador")]
        public ActionResult Edit([Bind(Include = "id_evento,id_venue,id_categoria,id_usuario,nombre,tipo,descripcion,imagen_url,fecha_inicio,fecha_fin,fecha_creacion,activo")] eventos evento)
        {
            ValidarEvento(evento);

            if (ModelState.IsValid)
            {
                db.Entry(evento).State = EntityState.Modified;
                db.SaveChanges();

                return RedirectToAction("Index");
            }

            CargarCombos(evento);
            return View(evento);
        }

        //GET: Eventos/Delete/5
        [ValidarRol("Admin", "Organizador")]
        public ActionResult Delete(int? id)
        {
            if(id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var evento = db.eventos
                .Include(e => e.venues)
                .Include(e => e.categorias)
                .Include(e => e.usuarios)
                .FirstOrDefault(e => e.id_evento == id);

            if(evento == null) 
                return HttpNotFound();

            return View(evento);
        }

        //POST Evento/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [ValidarRol("Admin", "Organizador")]
        public ActionResult DeleteConfirmed(int id)
        {
            eventos evento = db.eventos.Find(id);

            if(evento == null)
                return HttpNotFound();

            evento.activo = false;

            db.Entry(evento).State = EntityState.Modified;
            db.SaveChanges();

            return RedirectToAction("Index");
        }


        //METODO PARA CARGAR LOS EVENTOS EN COMBOBOX
        private void CargarCombos(eventos evento = null)
        {
            ViewBag.id_venue = new SelectList(
                db.venues.Where(v => v.activo == true).OrderBy(v => v.nombre),
                "id_venue",
                "nombre",
                evento?.id_venue
            );

            ViewBag.id_categoria = new SelectList(
                db.categorias.Where(c => c.activo == true).OrderBy(c => c.nombre),
                "id_categoria",
                "nombre",
                evento?.id_categoria
            );

            ViewBag.id_usuario = new SelectList(
                db.usuarios.Where(u => u.activo == true).OrderBy(u => u.nombre),
                "id_usuario",
                "nombre",
                evento?.id_usuario
            );
        }

        //METODO PARA VALIDAR EVENTOS
        private void ValidarEvento(eventos evento)
        {
            if (string.IsNullOrWhiteSpace(evento.nombre))
            {
                ModelState.AddModelError("nombre", "El nombre del evento es obligatorio.");
            }

            if (string.IsNullOrWhiteSpace(evento.tipo))
            {
                ModelState.AddModelError("tipo", "El tipo de evento es obligatorio.");
            }

            if (evento.fecha_fin != null && evento.fecha_fin < evento.fecha_inicio)
            {
                ModelState.AddModelError("fecha_fin", "La fecha final no puede ser menor que la fecha de inicio.");
            }
        }

        //METODO PARA LIBERAR RECURSOS
        protected override void Dispose(bool disposing)
        {
            if (disposing)
                db.Dispose();

            base.Dispose(disposing);
        }
    }
}