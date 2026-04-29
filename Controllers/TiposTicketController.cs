using System;
using System.Data.Entity;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net;

namespace GestionTickets.Controllers
{
    public class TiposTicketController : Controller
    {
        private gestion_ticketsEntities db = new gestion_ticketsEntities();

        // GET: TiposTicket
        public ActionResult Index()
        {
            var tiposTicket = db.sp_mostrar_tipos_ticket(null).ToList();
            return View(tiposTicket);
        }

        //GET: tiposTicket/PorEvento/5
        public ActionResult porEvento(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var evento = db.eventos.Find(id);

            if (evento == null)
                return HttpNotFound();

            ViewBag.EventoNombre = evento.nombre;
            ViewBag.IdEvento = evento.id_evento;

            var tickets = db.tipos_ticket
                .Include(t => t.eventos)
                .Include(t => t.secciones_venue)
                .Where(t => t.id_evento == id && t.activo == true)
                .OrderBy(t => t.precio)
                .ToList();

            return View(tickets);
        }

        //GET: TiposTicket/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var ticket = db.tipos_ticket
                .Include(t => t.eventos)
                .Include(t => t.secciones_venue)
                .FirstOrDefault(t => t.id_tipo_ticket == id);

            if (ticket == null)
                return HttpNotFound();

            return View(ticket);
        }

        //GET: TiposTicket/Create
        public ActionResult Create(int? idEvento)
        {
            CargarCombos(idEvento);
            return View();
        }

        //POST: tiposTicket/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id_evento,id_seccion,nombre,precio,moneda,cantidad_ticket,venta_inicio,venta_fin,activo")] tipos_ticket tipoTicket)
        {
            ValidarTipoTicket(tipoTicket);

            if (ModelState.IsValid)
            {
                if (string.IsNullOrWhiteSpace(tipoTicket.moneda))
                    tipoTicket.moneda = "USD";

                if (tipoTicket.activo == null)
                    tipoTicket.activo = true;

                db.tipos_ticket.Add(tipoTicket);
                db.SaveChanges();

                TempData["SuccessMessage"] = "El tipo de ticket se creo correctamente";

                return RedirectToAction("PorEvento", new { id = tipoTicket.id_evento });
            }

            CargarCombos(tipoTicket.id_evento, tipoTicket);
            return View(tipoTicket);
        }

        //GET: TiposTicket/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            tipos_ticket ticket = db.tipos_ticket.Find(id);

            if (ticket == null)
                return HttpNotFound();

            CargarCombos(ticket.id_evento, ticket);
            return View(ticket);
        }

        //POST tiposTicket/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id_tipo_ticket,id_evento,id_seccion,nombre,precio,moneda,cantidad_ticket,venta_inicio,venta_fin,activo")] tipos_ticket tipoTicket)
        {
            ValidarTipoTicket(tipoTicket);

            if (ModelState.IsValid)
            {
                if (string.IsNullOrWhiteSpace(tipoTicket.moneda))
                    tipoTicket.moneda = "USD";

                db.Entry(tipoTicket).State = EntityState.Modified;
                db.SaveChanges();

                TempData["SuccessMessage"] = "El tipo de ticket se actualizó correctamente.";

                return RedirectToAction("PorEvento", new { id = tipoTicket.id_evento });
            }

            CargarCombos(tipoTicket.id_evento, tipoTicket);
            return View(tipoTicket);
        }

        // GET: TiposTicket/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var ticket = db.tipos_ticket
                .Include(t => t.eventos)
                .Include(t => t.secciones_venue)
                .FirstOrDefault(t => t.id_tipo_ticket == id);

            if (ticket == null)
                return HttpNotFound();

            return View(ticket);
        }

        // POST: TiposTicket/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            tipos_ticket ticket = db.tipos_ticket.Find(id);

            if (ticket == null)
                return HttpNotFound();

            int idEvento = ticket.id_evento;

            ticket.activo = false;

            db.Entry(ticket).State = EntityState.Modified;
            db.SaveChanges();

            TempData["SuccessMessage"] = "El tipo de ticket se desactivó correctamente.";

            return RedirectToAction("PorEvento", new { id = idEvento });
        }

        private void CargarCombos(int? idEventoSeleccionado = null, tipos_ticket tipoTicket = null)
        {
            ViewBag.id_evento = new SelectList(
                db.eventos
                    .Where(e => e.activo == true)
                    .OrderBy(e => e.nombre),
                "id_evento",
                "nombre",
                idEventoSeleccionado ?? tipoTicket?.id_evento
            );

            ViewBag.id_seccion = new SelectList(
                db.secciones_venue
                    .Where(s => s.activo == true)
                    .OrderBy(s => s.nombre),
                "id_seccion",
                "nombre",
                tipoTicket?.id_seccion
            );
        }

        private void ValidarTipoTicket(tipos_ticket tipoTicket)
        {
            if (tipoTicket.id_evento <= 0)
                ModelState.AddModelError("id_evento", "Debe seleccionar un evento.");

            if (string.IsNullOrWhiteSpace(tipoTicket.nombre))
                ModelState.AddModelError("nombre", "El nombre del tipo de ticket es obligatorio.");

            if (tipoTicket.precio <= 0)
                ModelState.AddModelError("precio", "El precio debe ser mayor a cero.");

            if (tipoTicket.cantidad_ticket <= 0)
                ModelState.AddModelError("cantidad_ticket", "La cantidad de tickets debe ser mayor a cero.");

            if (tipoTicket.venta_inicio != null && tipoTicket.venta_fin != null && tipoTicket.venta_fin < tipoTicket.venta_inicio)
                ModelState.AddModelError("venta_fin", "La fecha final de venta no puede ser menor que la fecha inicial.");

            if (!string.IsNullOrWhiteSpace(tipoTicket.moneda) && tipoTicket.moneda.Length > 10)
                ModelState.AddModelError("moneda", "La moneda no puede tener más de 10 caracteres.");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                db.Dispose();

            base.Dispose(disposing);
        }
    }
}