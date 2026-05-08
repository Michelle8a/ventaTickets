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
    public class VenuesController : Controller
    {
        private gestion_ticketsEntities db = new gestion_ticketsEntities();

        // GET: Venues
        public ActionResult Index()
        {
            var venues = db.sp_mostrar_venues(null).ToList();
            return View(venues);
        }

        // GET: Venues/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var venue = db.venues
                .Include(v => v.ciudades)
                .FirstOrDefault(v => v.id_venue == id);

            if (venue == null)
                return HttpNotFound();

            return View(venue);
        }

        // GET: Venues/Create
        [ValidarRol("Admin", "Organizador")]
        public ActionResult Create()
        {
            CargarCombos();
            return View();
        }

        // POST: Venues/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidarRol("Admin", "Organizador")]
        public ActionResult Create([Bind(Include = "id_ciudad,nombre,tipo,direccion,capacidad,latitud,longitud,imagen_url,activo")] venues venue)
        {
            ValidarVenue(venue);

            if (ModelState.IsValid)
            {
                if (venue.activo == null)
                    venue.activo = true;

                db.venues.Add(venue);
                db.SaveChanges();

                return RedirectToAction("Index");
            }

            CargarCombos(venue);
            return View(venue);
        }

        // GET: Venues/Edit/5
        [ValidarRol("Admin", "Organizador")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            venues venue = db.venues.Find(id);

            if (venue == null)
                return HttpNotFound();

            CargarCombos(venue);
            return View(venue);
        }

        // POST: Venues/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidarRol("Admin", "Organizador")]
        public ActionResult Edit([Bind(Include = "id_venue,id_ciudad,nombre,tipo,direccion,capacidad,latitud,longitud,imagen_url,activo")] venues venue)
        {
            ValidarVenue(venue);

            if (ModelState.IsValid)
            {
                db.Entry(venue).State = EntityState.Modified;
                db.SaveChanges();

                return RedirectToAction("Index");
            }

            CargarCombos(venue);
            return View(venue);
        }

        // GET: Venues/Delete/5
        [ValidarRol("Admin", "Organizador")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var venue = db.venues
                .Include(v => v.ciudades)
                .FirstOrDefault(v => v.id_venue == id);

            if (venue == null)
                return HttpNotFound();

            return View(venue);
        }

        // POST: Venues/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [ValidarRol("Admin", "Organizador")]
        public ActionResult DeleteConfirmed(int id)
        {
            venues venue = db.venues.Find(id);

            if (venue == null)
                return HttpNotFound();

            venue.activo = false;

            db.Entry(venue).State = EntityState.Modified;
            db.SaveChanges();

            return RedirectToAction("Index");
        }

        // METODO PARA CARGAR COMBOS
        private void CargarCombos(venues venue = null)
        {
            ViewBag.id_ciudad = new SelectList(
                db.ciudades.Where(c => c.activo == true).OrderBy(c => c.nombre),
                "id_ciudad",
                "nombre",
                venue?.id_ciudad
            );
        }

        // METODO PARA VALIDAR VENUE
        private void ValidarVenue(venues venue)
        {
            if (string.IsNullOrWhiteSpace(venue.nombre))
                ModelState.AddModelError("nombre", "El nombre del venue es obligatorio.");

            if (string.IsNullOrWhiteSpace(venue.tipo))
                ModelState.AddModelError("tipo", "El tipo de venue es obligatorio.");

            if (venue.capacidad != null && venue.capacidad <= 0)
                ModelState.AddModelError("capacidad", "La capacidad debe ser mayor a 0.");
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