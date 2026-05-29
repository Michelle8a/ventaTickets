using GestionTickets.Models;
using GestionTickets.Permisos;
using Stripe;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace GestionTickets.Controllers
{
    [ValidarSesion]
    public class PagosController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // ── Inicializa Stripe con la clave secreta ───────────────────────
        private void ConfigurarStripe()
        {
            StripeConfiguration.ApiKey = ConfigurationManager.AppSettings["Stripe:SecretKey"];
        }

        // ── Calcula el total de una compra (sin propina) ─────────────────
        private decimal CalcularTotal(Compra compra, decimal envio = 1.00m, decimal propina = 0m)
        {
            var subtotal = compra.Cantidad * compra.PrecioUnitario;
            var cargos   = compra.Cantidad * 2m;
            return subtotal + cargos + envio + propina;
        }

        // GET: Pagos
        public ActionResult Index()
        {
            var pagos = db.pagos
                          .Where(p => p.Activo)
                          .OrderByDescending(p => p.FechaPago)
                          .ToList();

            // Hidratar campos calculados para la vista
            foreach (var p in pagos)
            {
                var compra = db.compras.Find(p.IdCompra);
                if (compra != null)
                {
                    p.Subtotal = compra.Cantidad * compra.PrecioUnitario;
                    p.Cargos   = compra.Cantidad * 2m;
                    p.Envio    = 1.00m;
                }
            }

            return View(pagos);
        }

        // GET: Pagos/Create?compraId=5
        public ActionResult Create(int? compraId)
        {
            if (compraId == null)
            {
                TempData["Error"] = "Debe iniciar una compra antes de pagar.";
                return RedirectToAction("Create", "Compra");
            }

            var compra = db.compras.Find(compraId.Value);
            if (compra == null) return HttpNotFound();

            var pago = new Pago
            {
                CompraId = compra.Id,
                Cantidad = compra.Cantidad,
                Subtotal = compra.Cantidad * compra.PrecioUnitario,
                Cargos   = compra.Cantidad * 2m,
                Envio    = 1.00m,
                Propina  = 0m
            };

            ViewBag.StripePublishableKey = ConfigurationManager.AppSettings["Stripe:PublishableKey"];
            return View(pago);
        }

        // POST Ajax: Pagos/CreateIntent — crea un PaymentIntent con el monto final (incluye propina)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult CreateIntent(int compraId, decimal propina)
        {
            var compra = db.compras.Find(compraId);
            if (compra == null)
                return Json(new { error = "Compra no encontrada." });

            if (propina < 0) propina = 0m;
            var total = CalcularTotal(compra, 1.00m, propina);

            try
            {
                ConfigurarStripe();
                var service = new PaymentIntentService();
                var pi = service.Create(new PaymentIntentCreateOptions
                {
                    Amount   = (long)Math.Round(total * 100), // Stripe usa centavos
                    Currency = "usd",
                    PaymentMethodTypes = new List<string> { "card" },
                    Metadata = new Dictionary<string, string>
                    {
                        { "compra_id",    compra.Id.ToString() },
                        { "codigo_orden", compra.CodigoOrden ?? "" }
                    }
                });

                return Json(new { clientSecret = pi.ClientSecret });
            }
            catch (StripeException ex)
            {
                return Json(new { error = ex.StripeError?.Message ?? ex.Message });
            }
        }

        // POST: Pagos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Pago pago, string stripePaymentIntentId)
        {
            var compra = db.compras.Find(pago.IdCompra);
            if (compra == null) return HttpNotFound();

            // Recalcular siempre en servidor
            pago.Cantidad = compra.Cantidad;
            pago.Subtotal = compra.Cantidad * compra.PrecioUnitario;
            pago.Cargos   = compra.Cantidad * 2m;
            if (pago.Envio  < 0) pago.Envio  = 1.00m;
            if (pago.Propina < 0) pago.Propina = 0m;

            if (pago.Metodo == "card")
            {
                // Limpiar validaciones manuales de tarjeta (ahora las maneja Stripe)
                ModelState.Remove("TitularTarjeta");
                ModelState.Remove("NumeroTarjeta");
                ModelState.Remove("Vencimiento");
                ModelState.Remove("Cvv");

                if (string.IsNullOrWhiteSpace(stripePaymentIntentId))
                {
                    ModelState.AddModelError("", "No se recibió confirmación del pago con tarjeta.");
                }
                else
                {
                    try
                    {
                        ConfigurarStripe();
                        var piService = new PaymentIntentService();
                        var pi = piService.Get(stripePaymentIntentId, new PaymentIntentGetOptions
                        {
                            Expand = new List<string> { "payment_method" }
                        });

                        if (pi.Status != "succeeded")
                        {
                            ModelState.AddModelError("", $"El pago no fue aprobado por Stripe. Estado: {pi.Status}");
                        }
                        else
                        {
                            pago.StripePaymentIntentId = stripePaymentIntentId;

                            // Guardar datos reales de la tarjeta desde la respuesta de Stripe
                            var card = pi.PaymentMethod?.Card;
                            var tarjeta = new MetodoPagoTarjeta
                            {
                                Titular          = pi.PaymentMethod?.BillingDetails?.Name ?? pago.TitularTarjeta ?? "—",
                                NumeroEnmascarado = $"**** **** **** {card?.Last4 ?? "****"}",
                                TipoTarjeta      = CapitalizarMarca(card?.Brand),
                                FechaExpiracion  = card != null
                                                    ? $"{card.ExpMonth:D2}/{card.ExpYear % 100:D2}"
                                                    : "—",
                                Activo = true
                            };
                            db.metodos_pago_tarjeta.Add(tarjeta);
                            db.SaveChanges();
                            pago.IdMetodoTarjeta = tarjeta.IdMetodoTarjeta;
                        }
                    }
                    catch (StripeException ex)
                    {
                        ModelState.AddModelError("", $"Error de Stripe: {ex.StripeError?.Message ?? ex.Message}");
                    }
                }
            }
            else // cash
            {
                ModelState.Remove("TitularTarjeta");
                ModelState.Remove("NumeroTarjeta");
                ModelState.Remove("Vencimiento");
                ModelState.Remove("Cvv");

                var cashDefault = db.metodos_pago_cash.FirstOrDefault(c => c.Activo);
                pago.IdMetodoCash = cashDefault?.IdMetodoCash;
            }

            if (!ModelState.IsValid)
            {
                ViewBag.StripePublishableKey = ConfigurationManager.AppSettings["Stripe:PublishableKey"];
                // Rehidratar para la vista
                pago.Subtotal = compra.Cantidad * compra.PrecioUnitario;
                pago.Cargos   = compra.Cantidad * 2m;
                return View(pago);
            }

            // ── Guardar el pago ──────────────────────────────────────────
            pago.IdUsuario   = compra.IdUsuario;
            pago.Monto       = pago.Total;   // Subtotal + Cargos + Envio + Propina
            pago.Moneda      = "USD";
            pago.FechaPago   = DateTime.Now;
            pago.Activo      = true;
            pago.Estado      = pago.Metodo == "cash" ? "pendiente" : "aprobado";
            pago.Cvv         = null;

            db.pagos.Add(pago);

            // ── Actualizar estado de la compra ───────────────────────────
            compra.Estado = pago.Metodo == "cash" ? "pendiente_entrega" : "pagada";
            db.Entry(compra).State = EntityState.Modified;

            // ── Decrementar disponibilidad de tickets ────────────────────
            var tipo = db.tipos_ticket.Find(compra.IdTipoTicket);
            if (tipo != null)
            {
                tipo.CantidadTicket = Math.Max(0, tipo.CantidadTicket - compra.Cantidad);
                db.Entry(tipo).State = EntityState.Modified;
            }

            db.SaveChanges();

            return RedirectToAction("Details", new { id = pago.IdPago });
        }

        // GET: Pagos/Details/5
        public ActionResult Details(int id)
        {
            var pago = db.pagos.Find(id);
            if (pago == null) return HttpNotFound();

            // Hidratar campos calculados
            var compra = db.compras.Find(pago.IdCompra);
            if (compra != null)
            {
                pago.Cantidad = compra.Cantidad;
                pago.Subtotal = compra.Cantidad * compra.PrecioUnitario;
                pago.Cargos   = compra.Cantidad * 2m;
                pago.Envio    = 1.00m;
                ViewBag.CodigoOrden = compra.CodigoOrden;
            }

            // Cargar datos de la tarjeta si aplica
            if (pago.IdMetodoTarjeta.HasValue)
            {
                var tarjeta = db.metodos_pago_tarjeta.Find(pago.IdMetodoTarjeta.Value);
                ViewBag.NumeroEnmascarado = tarjeta?.NumeroEnmascarado;
                ViewBag.TipoTarjeta       = tarjeta?.TipoTarjeta;
                ViewBag.TitularTarjeta    = tarjeta?.Titular;
            }

            // Verificar si tiene reembolso
            ViewBag.TieneReembolso = db.reembolsos.Any(r => r.IdPago == id);

            return View(pago);
        }

        // GET: Pagos/Edit/5
        public ActionResult Edit(int id)
        {
            var pago = db.pagos.Find(id);
            if (pago == null) return HttpNotFound();
            return View(pago);
        }

        // POST: Pagos/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Pago pago)
        {
            if (!ModelState.IsValid) return View(pago);
            db.Entry(pago).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        // GET: Pagos/Delete/5
        public ActionResult Delete(int id)
        {
            var pago = db.pagos.Find(id);
            if (pago == null) return HttpNotFound();
            return View(pago);
        }

        // POST: Pagos/Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var pago = db.pagos.Find(id);
            if (pago != null)
            {
                pago.Activo = false;
                db.Entry(pago).State = EntityState.Modified;
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        private string CapitalizarMarca(string brand)
        {
            if (string.IsNullOrEmpty(brand)) return "Otra";
            return brand.Substring(0, 1).ToUpper() + brand.Substring(1).ToLower();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
