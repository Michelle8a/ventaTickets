using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace GestionTickets.Controllers
{
    public class PagosController : Controller
    {
        // Simulación de base de datos (temporal)
        static List<Pago> listaPagos = new List<Pago>();

        // 🔹 GET: Pagos
        public ActionResult Index()
        {
            return View(listaPagos);
        }

        // 🔹 GET: Pagos/Create
        public ActionResult Create()
        {
            return View();
        }

        // 🔹 POST: Pagos/Create
        [HttpPost]
        public ActionResult Create(Pago pago)
        {
            pago.IdPago = listaPagos.Count + 1;
            listaPagos.Add(pago);

            return RedirectToAction("Index");
        }

        // 🔹 GET: Pagos/Edit/5
        public ActionResult Edit(int id)
        {
            var pago = listaPagos.FirstOrDefault(p => p.IdPago == id);
            return View(pago);
        }

        // 🔹 POST: Pagos/Edit
        [HttpPost]
        public ActionResult Edit(Pago pago)
        {
            var pagoExistente = listaPagos.FirstOrDefault(p => p.IdPago == pago.IdPago);

            if (pagoExistente != null)
            {
                pagoExistente.MetodoPago = pago.MetodoPago;
                pagoExistente.NumeroTarjeta = pago.NumeroTarjeta;
            }

            return RedirectToAction("Index");
        }

        // 🔹 GET: Pagos/Details/5
        public ActionResult Details(int id)
        {
            var pago = listaPagos.FirstOrDefault(p => p.IdPago == id);
            return View(pago);
        }

        // 🔹 GET: Pagos/Delete/5
        public ActionResult Delete(int id)
        {
            var pago = listaPagos.FirstOrDefault(p => p.IdPago == id);
            return View(pago);
        }

        // 🔹 POST: Pagos/Delete
        [HttpPost]
        public ActionResult Delete(Pago pago)
        {
            var pagoExistente = listaPagos.FirstOrDefault(p => p.IdPago == pago.IdPago);

            if (pagoExistente != null)
            {
                listaPagos.Remove(pagoExistente);
            }

            return RedirectToAction("Index");
        }
    }

    // 🔥 Modelo simple (temporal)
    public class Pago
    {
        public int IdPago { get; set; }
        public string MetodoPago { get; set; }
        public string NumeroTarjeta { get; set; }
    }
}