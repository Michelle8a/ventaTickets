using GestionTickets.Permisos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace GestionTickets.Controllers
{
    public class DashboardController : Controller
    {
        // =========================
        // DASHBOARD ADMIN
        // =========================
        [ValidarRol("Admin")]
        public ActionResult ReporteAdmin()
        {
            return View();
        }

        // =========================
        // REPORTES ADMIN
        // =========================
        [ValidarRol("Admin")]

        public ActionResult VentasTotales()
        {
            return View();
        }
        [ValidarRol("Admin")]

        public ActionResult EventosVendidos()
        {
            return View();
        }
                [ValidarRol("Admin")]
        public ActionResult VentasPais()
        {
            return View();
        }
        [ValidarRol("Admin")]
        public ActionResult MetodosPago()
        {
            return View();
        }
        [ValidarRol("Admin")]
        public ActionResult Reembolsos()
        {
            return View();
        }
        [ValidarRol("Admin")]
        public ActionResult IngresosMensuales()
        {
            return View();
        }

        // =========================
        // DASHBOARD ORGANIZADOR
        // =========================
        [ValidarRol("Organizador")]
        public ActionResult ReporteOrganizador()
        {
            return View();
        }
        [ValidarRol("Organizador")]
        // =========================
        // REPORTES ORGANIZADOR
        // =========================

        public ActionResult TicketsVendidos()
        {
            return View();
        }
        [ValidarRol("Organizador")]
        public ActionResult MisIngresos()
        {
            return View();
        }
        [ValidarRol("Organizador")]
        public ActionResult EventosPopulares()
        {
            return View();
        }
        [ValidarRol("Organizador")]
        public ActionResult OcupacionVenue()
        {
            return View();
        }
        [ValidarRol("Organizador")]
        public ActionResult PresentacionesEvento()
        {
            return View();
        }
        [ValidarRol("Organizador")]
        public ActionResult UsoDescuentos()
        {
            return View();
        }
    }
}