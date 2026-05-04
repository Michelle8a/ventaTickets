using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace GestionTickets.Controllers
{
    public class ArtistasController : Controller
    {
        private gestion_ticketsEntities db = new gestion_ticketsEntities();

        // GET: Artistas
        public ActionResult Index()
        {
            var artistas = db.artistas
                .Where(a => a.activo == true)
                .OrderBy(a => a.nombre)
                .ToList();

            return View(artistas);
        }

        // GET: Artistas/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            artistas artista = db.artistas.Find(id);

            if (artista == null)
                return HttpNotFound();

            return View(artista);
        }

        // GET: Artistas/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Artistas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "nombre,genero,descripcion,imagen_url,activo")] artistas artista)
        {
            ValidarArtista(artista);

            if (ModelState.IsValid)
            {
                if (artista.activo == null)
                    artista.activo = true;

                db.artistas.Add(artista);
                db.SaveChanges();

                TempData["SuccessMessage"] = "El artista se creó correctamente.";

                return RedirectToAction("Index");
            }

            return View(artista);
        }

        // GET: Artistas/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            artistas artista = db.artistas.Find(id);

            if (artista == null)
                return HttpNotFound();

            return View(artista);
        }

        // POST: Artistas/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id_artista,nombre,genero,descripcion,imagen_url,activo")] artistas artista)
        {
            ValidarArtista(artista);

            if (ModelState.IsValid)
            {
                db.Entry(artista).State = EntityState.Modified;
                db.SaveChanges();

                TempData["SuccessMessage"] = "El artista se actualizó correctamente.";

                return RedirectToAction("Index");
            }

            return View(artista);
        }

        // GET: Artistas/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            artistas artista = db.artistas.Find(id);

            if (artista == null)
                return HttpNotFound();

            return View(artista);
        }

        // POST: Artistas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            artistas artista = db.artistas.Find(id);

            if (artista == null)
                return HttpNotFound();

            // Eliminado lógico
            artista.activo = false;

            db.Entry(artista).State = EntityState.Modified;
            db.SaveChanges();

            TempData["SuccessMessage"] = "El artista se desactivó correctamente.";

            return RedirectToAction("Index");
        }

        private void ValidarArtista(artistas artista)
        {
            if (string.IsNullOrWhiteSpace(artista.nombre))
            {
                ModelState.AddModelError("nombre", "El nombre del artista es obligatorio.");
            }

            if (!string.IsNullOrWhiteSpace(artista.nombre) && artista.nombre.Length > 150)
            {
                ModelState.AddModelError("nombre", "El nombre no puede tener más de 150 caracteres.");
            }

            if (!string.IsNullOrWhiteSpace(artista.genero) && artista.genero.Length > 50)
            {
                ModelState.AddModelError("genero", "El género no puede tener más de 50 caracteres.");
            }

            if (!string.IsNullOrWhiteSpace(artista.imagen_url) && artista.imagen_url.Length > 255)
            {
                ModelState.AddModelError("imagen_url", "La URL de imagen no puede tener más de 255 caracteres.");
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                db.Dispose();

            base.Dispose(disposing);
        }
    }
}