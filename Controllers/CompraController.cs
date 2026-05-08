using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using GestionTickets.Models;
using System.Web.Mvc;

namespace GestionTickets.Controllers
{
        public class CompraController : Controller
        {
        private ApplicationDbContext db = new ApplicationDbContext();

        // LISTADO
        public ActionResult Index()
            {
                return View(db.compras.OrderByDescending(c => c.Fecha).ToList());
            }

            // FORM
            public ActionResult Create()
            {
                return View(new Compra());
            }

            // PROCESAR COMPRA
            [HttpPost]
            [ValidateAntiForgeryToken]
            public ActionResult Create(Compra compra)
            {
                // 🔒 Validación extra
                if (compra.Cantidad > 4)
                {
                    ModelState.AddModelError("", "Máximo 4 boletos permitidos.");
                }

            if (compra.MetodoPago == "Efectivo")
            {
                compra.NumeroTarjeta = null;
            }

            if (ModelState.IsValid)
                {
                compra.Subtotal = compra.Cantidad * compra.PrecioUnitario;
                compra.Cargos = compra.Cantidad * compra.CargoServicio;
                compra.Total = compra.Subtotal + compra.Cargos;
                compra.Fecha = DateTime.Now;
                db.compras.Add(compra);
                db.SaveChanges();

                return RedirectToAction("Detalle", new { id = compra.Id });
                }

                return View(compra);
            }

            // DETALLE
            public ActionResult Detalle(int id)
            {
                var compra = db.compras.Find(id);
                if (compra == null) return HttpNotFound();

                return View(compra);
            }

            // PDF
           // public ActionResult GenerarPDF(int id)
            //{
              //  var compra = db.compras.Find(id);
                //return new ViewAsPdf("Detalle", compra)
                //{
                  //  FileName = $"Compra_{id}.pdf"
               // };
            //
        }
    }