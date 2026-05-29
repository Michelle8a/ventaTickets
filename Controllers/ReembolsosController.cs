using GestionTickets.Models;
using GestionTickets.Permisos;
using Stripe;
using System;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace GestionTickets.Controllers
{
    [ValidarSesion]
    public class ReembolsosController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        private void ConfigurarStripe()
        {
            StripeConfiguration.ApiKey = ConfigurationManager.AppSettings["Stripe:SecretKey"];
        }

        // GET: Reembolsos
        public ActionResult Index()
        {
            var reembolsos = db.reembolsos
                               .OrderByDescending(r => r.FechaSolicitud)
                               .ToList();

            // Hidratar datos de navegación para la vista
            foreach (var r in reembolsos)
            {
                r.Pago   = db.pagos.Find(r.IdPago);
                r.Compra = db.compras.Find(r.IdCompra);
            }

            return View(reembolsos);
        }

        // GET: Reembolsos/Create?pagoId=5
        public ActionResult Create(int? pagoId)
        {
            if (pagoId == null) return RedirectToAction("Index", "Pagos");

            var pago = db.pagos.Find(pagoId.Value);
            if (pago == null) return HttpNotFound();

            if (pago.Estado != "aprobado")
            {
                TempData["Error"] = "Solo se pueden reembolsar pagos con estado 'aprobado'.";
                return RedirectToAction("Details", "Pagos", new { id = pagoId });
            }

            var compra = db.compras.Find(pago.IdCompra);

            // Hidratar campos calculados del pago para mostrar el resumen
            if (compra != null)
            {
                pago.Subtotal = compra.Cantidad * compra.PrecioUnitario;
                pago.Cargos   = compra.Cantidad * 2m;
                pago.Envio    = 1.00m;
            }

            ViewBag.Pago   = pago;
            ViewBag.Compra = compra;

            var reembolso = new Reembolso
            {
                IdPago   = pago.IdPago,
                IdCompra = pago.IdCompra,
                Monto    = pago.Monto  // Pre-rellenar con monto total
            };

            return View(reembolso);
        }

        // POST: Reembolsos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Reembolso reembolso)
        {
            var pago   = db.pagos.Find(reembolso.IdPago);
            var compra = pago != null ? db.compras.Find(pago.IdCompra) : null;

            if (pago == null) return HttpNotFound();

            // Validar monto
            if (reembolso.Monto <= 0 || reembolso.Monto > pago.Monto)
                ModelState.AddModelError("Monto", $"El monto debe estar entre $0.01 y ${pago.Monto:0.00}.");

            if (!ModelState.IsValid)
            {
                if (compra != null)
                {
                    pago.Subtotal = compra.Cantidad * compra.PrecioUnitario;
                    pago.Cargos   = compra.Cantidad * 2m;
                    pago.Envio    = 1.00m;
                }
                ViewBag.Pago   = pago;
                ViewBag.Compra = compra;
                return View(reembolso);
            }

            // ── Procesar reembolso en Stripe (solo si el pago fue con tarjeta) ──
            if (!string.IsNullOrWhiteSpace(pago.StripePaymentIntentId))
            {
                try
                {
                    ConfigurarStripe();
                    var refundService = new RefundService();
                    var refund = refundService.Create(new RefundCreateOptions
                    {
                        PaymentIntent = pago.StripePaymentIntentId,
                        Amount        = (long)Math.Round(reembolso.Monto * 100)
                    });

                    reembolso.StripeRefundId = refund.Id;
                    reembolso.Estado         = refund.Status == "succeeded" ? "completado" : "pendiente";
                    reembolso.FechaProceso   = reembolso.Estado == "completado" ? DateTime.Now : (DateTime?)null;
                }
                catch (StripeException ex)
                {
                    ModelState.AddModelError("", $"Error al procesar el reembolso en Stripe: {ex.StripeError?.Message ?? ex.Message}");
                    if (compra != null)
                    {
                        pago.Subtotal = compra.Cantidad * compra.PrecioUnitario;
                        pago.Cargos   = compra.Cantidad * 2m;
                    }
                    ViewBag.Pago   = pago;
                    ViewBag.Compra = compra;
                    return View(reembolso);
                }
            }
            else
            {
                // Pago en efectivo — reembolso manual, queda pendiente
                reembolso.Estado = "pendiente";
            }

            reembolso.FechaSolicitud = DateTime.Now;
            db.reembolsos.Add(reembolso);

            // ── Actualizar estado del pago ───────────────────────────────
            pago.Estado = "reembolsado";
            db.Entry(pago).State = EntityState.Modified;

            // ── Actualizar estado de la compra y restaurar stock ─────────
            if (compra != null)
            {
                compra.Estado = "cancelada";
                db.Entry(compra).State = EntityState.Modified;

                var tipo = db.tipos_ticket.Find(compra.IdTipoTicket);
                if (tipo != null)
                {
                    tipo.CantidadTicket += compra.Cantidad;
                    db.Entry(tipo).State = EntityState.Modified;
                }
            }

            db.SaveChanges();

            TempData["Exito"] = "Reembolso procesado correctamente.";
            return RedirectToAction("Details", new { id = reembolso.IdReembolso });
        }

        // GET: Reembolsos/Details/5
        public ActionResult Details(int id)
        {
            var reembolso = db.reembolsos.Find(id);
            if (reembolso == null) return HttpNotFound();

            reembolso.Pago   = db.pagos.Find(reembolso.IdPago);
            reembolso.Compra = db.compras.Find(reembolso.IdCompra);

            if (reembolso.Pago != null && reembolso.Compra != null)
            {
                reembolso.Pago.Subtotal = reembolso.Compra.Cantidad * reembolso.Compra.PrecioUnitario;
                reembolso.Pago.Cargos   = reembolso.Compra.Cantidad * 2m;
                reembolso.Pago.Envio    = 1.00m;
            }

            return View(reembolso);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
