using GestionTickets.Models;
using GestionTickets.Permisos;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace GestionTickets.Controllers
{
    [ValidarSesion]
    public class PagosController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // Lista temporal — luego la reemplazamos con DbSet<Pago>
        static List<Pago> listaPagos = new List<Pago>();

        // GET: Pagos
        public ActionResult Index()
        {
            return View(listaPagos);
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
                Subtotal = compra.Subtotal,
                Cargos = compra.Cargos,
                Envio = 1.00m,
                Propina = 0.00m
            };

            return View(pago);
        }

        // POST: Pagos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Pago pago)
        {
            var compra = db.compras.Find(pago.IdCompra);
            if (compra == null) return HttpNotFound();

            // Recalcular montos en servidor
            pago.Cantidad = compra.Cantidad;
            pago.Subtotal = compra.Subtotal;
            pago.Cargos = compra.Cargos;
            if (pago.Envio < 0) pago.Envio = 1.00m;
            if (pago.Propina < 0) pago.Propina = 0m;

            // Validaciones según método
            if (pago.Metodo == "card")
            {
                if (string.IsNullOrWhiteSpace(pago.TitularTarjeta))
                    ModelState.AddModelError("TitularTarjeta", "El titular es obligatorio.");
                if (string.IsNullOrWhiteSpace(pago.NumeroTarjeta) ||
                    pago.NumeroTarjeta.Replace(" ", "").Length < 12)
                    ModelState.AddModelError("NumeroTarjeta", "Número de tarjeta inválido.");
                if (string.IsNullOrWhiteSpace(pago.Vencimiento))
                    ModelState.AddModelError("Vencimiento", "Vencimiento obligatorio.");
                if (string.IsNullOrWhiteSpace(pago.Cvv))
                    ModelState.AddModelError("Cvv", "CVV obligatorio.");
            }
            else
            {
                ModelState.Remove("TitularTarjeta");
                ModelState.Remove("NumeroTarjeta");
                ModelState.Remove("Vencimiento");
                ModelState.Remove("Cvv");
            }

            if (!ModelState.IsValid)
                return View(pago);

            // ─── Si es tarjeta, crear el método primero ─────────
            int? idMetodoTarjeta = null;
            int? idMetodoCash = null;
            int? idMetodoTransferencia = null;

            if (pago.Metodo == "card")
            {
                var numeroLimpio = pago.NumeroTarjeta.Replace(" ", "");
                var enmascarado = "**** **** **** " + numeroLimpio.Substring(numeroLimpio.Length - 4);

                // Detectar tipo de tarjeta
                string tipoTarjeta = "Otra";
                if (numeroLimpio.StartsWith("4")) tipoTarjeta = "Visa";
                else if (numeroLimpio.StartsWith("5") || numeroLimpio.StartsWith("2")) tipoTarjeta = "Mastercard";
                else if (numeroLimpio.StartsWith("3")) tipoTarjeta = "Amex";

                var tarjeta = new MetodoPagoTarjeta
                {
                    Titular = pago.TitularTarjeta,
                    NumeroEnmascarado = enmascarado,
                    TipoTarjeta = tipoTarjeta,
                    FechaExpiracion = pago.Vencimiento,
                    Activo = true
                };
                db.metodos_pago_tarjeta.Add(tarjeta);
                db.SaveChanges();  // Para obtener el ID

                idMetodoTarjeta = tarjeta.IdMetodoTarjeta;
            }
            else if (pago.Metodo == "cash")
            {
                // Usa el primer punto de pago activo (o el predeterminado)
                var cashDefault = db.metodos_pago_cash.FirstOrDefault(c => c.Activo);
                idMetodoCash = cashDefault?.IdMetodoCash;
            }

            // ─── Preparar el pago ─────────────────────────────
            pago.IdUsuario = compra.IdUsuario;
            pago.Moneda = "USD";
            pago.FechaPago = DateTime.Now;
            pago.Activo = true;
            pago.Estado = pago.Metodo == "cash" ? "pendiente" : "aprobado";
            pago.IdMetodoTarjeta = idMetodoTarjeta;
            pago.IdMetodoCash = idMetodoCash;
            pago.IdMetodoTransferencia = idMetodoTransferencia;
            pago.Cvv = null;

            db.pagos.Add(pago);

            // Marcar compra como pagada
            compra.Estado = pago.Metodo == "cash" ? "pendiente_entrega" : "pagada";
            db.Entry(compra).State = EntityState.Modified;

            db.SaveChanges();

            return RedirectToAction("Details", new { id = pago.IdPago });
        }

        // GET: Pagos/Details/5
        public ActionResult Details(int id)
        {
            var pago = listaPagos.FirstOrDefault(p => p.IdPago == id);
            if (pago == null) return HttpNotFound();
            return View(pago);
        }

        // GET: Pagos/Edit/5
        public ActionResult Edit(int id)
        {
            var pago = listaPagos.FirstOrDefault(p => p.IdPago == id);
            if (pago == null) return HttpNotFound();
            return View(pago);
        }

        // POST: Pagos/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Pago pago)
        {
            if (!ModelState.IsValid) return View(pago);

            var existing = listaPagos.FirstOrDefault(p => p.IdPago == pago.IdPago);
            if (existing != null)
            {
                existing.MetodoPago = pago.MetodoPago;
                existing.TitularTarjeta = pago.TitularTarjeta;
                existing.NumeroTarjeta = pago.NumeroTarjeta;
                existing.Vencimiento = pago.Vencimiento;
                existing.Nombre = pago.Nombre;
                existing.Telefono = pago.Telefono;
                existing.Direccion = pago.Direccion;
                existing.Notas = pago.Notas;
                existing.Propina = pago.Propina;
                existing.Estado = pago.Estado;
            }
            return RedirectToAction("Index");
        }

        // GET: Pagos/Delete/5
        public ActionResult Delete(int id)
        {
            var pago = listaPagos.FirstOrDefault(p => p.IdPago == id);
            if (pago == null) return HttpNotFound();
            return View(pago);
        }

        // POST: Pagos/Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var pago = listaPagos.FirstOrDefault(p => p.IdPago == id);
            if (pago != null) listaPagos.Remove(pago);
            return RedirectToAction("Index");
        }
    }
}