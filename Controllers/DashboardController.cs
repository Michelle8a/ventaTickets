using GestionTickets.Models;
using GestionTickets.Permisos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace GestionTickets.Controllers
{
    public class DashboardController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // =========================================================
        // DASHBOARD ADMIN
        // =========================================================
        [ValidarRol("Admin")]
        public ActionResult ReporteAdmin()
        {
            var vm = new DashboardAdminViewModel
            {
                TotalVentas = db.pagos.Where(p => p.Activo && p.Estado == "aprobado").Sum(p => (decimal?)p.Monto) ?? 0,
                TotalTickets = db.compras.Where(c => c.Activo).Sum(c => (int?)c.Cantidad) ?? 0,
                TotalPagos = db.pagos.Count(p => p.Activo),
                TotalReembolsos = db.reembolsos.Count(r => r.Estado == "aprobado")
            };
            return View(vm);
        }

        // =========================================================
        // REPORTE: Ventas Totales (Admin)
        // =========================================================
        [ValidarRol("Admin")]
        public ActionResult VentasTotales()
        {
            var pagos = db.pagos
                          .Where(p => p.Activo && p.Estado == "aprobado")
                          .OrderByDescending(p => p.FechaPago)
                          .ToList();

            var vm = new VentasTotalesViewModel
            {
                TotalIngresos = pagos.Sum(p => p.Monto),
                TotalPagos = pagos.Count,
                PorMetodo = pagos
                                    .GroupBy(p => p.Metodo ?? "otro")
                                    .Select(g => new MetodoResumen
                                    {
                                        Metodo = g.Key,
                                        Total = g.Sum(x => x.Monto),
                                        Cantidad = g.Count()
                                    }).ToList(),
                UltimosPagos = pagos.Take(20).ToList()
            };

            return View(vm);
        }

        // =========================================================
        // REPORTE: Eventos Más Vendidos (Admin)
        // =========================================================
        [ValidarRol("Admin")]
        public ActionResult EventosVendidos()
        {
            // Cruzamos compras → tipos_ticket para agrupar por nombre de ticket
            var datos = db.compras
                          .Where(c => c.Activo)
                          .GroupBy(c => c.IdTipoTicket)
                          .Select(g => new
                          {
                              IdTipo = g.Key,
                              Cantidad = g.Sum(x => x.Cantidad),
                              Ingresos = g.Sum(x => (decimal)x.Cantidad * x.PrecioUnitario)
                          })
                          .OrderByDescending(x => x.Cantidad)
                          .ToList();

            var tiposDict = db.tipos_ticket.ToList().ToDictionary(t => t.IdTipoTicket);

            var vm = datos.Select(d => new EventoVendidoResumen
            {
                NombreTicket = tiposDict.ContainsKey(d.IdTipo) ? tiposDict[d.IdTipo].Nombre : "Desconocido",
                Cantidad = d.Cantidad,
                Ingresos = d.Ingresos
            }).ToList();

            return View(vm);
        }

        // =========================================================
        // REPORTE: Métodos de Pago (Admin)
        // =========================================================
        [ValidarRol("Admin")]
        public ActionResult MetodosPago()
        {
            var pagos = db.pagos.Where(p => p.Activo).ToList();

            var vm = pagos
                     .GroupBy(p => p.Metodo ?? "otro")
                     .Select(g => new MetodoResumen
                     {
                         Metodo = g.Key,
                         Total = g.Sum(x => x.Monto),
                         Cantidad = g.Count()
                     })
                     .OrderByDescending(x => x.Total)
                     .ToList();

            return View(vm);
        }

        // =========================================================
        // REPORTE: Reembolsos (Admin)
        // =========================================================
        [ValidarRol("Admin")]
        public ActionResult Reembolsos()
        {
            var reembolsos = db.reembolsos.OrderByDescending(r => r.FechaSolicitud).ToList();

            var vm = new ReembolsosReporteViewModel
            {
                TotalSolicitado = reembolsos.Sum(r => r.Monto),
                Aprobados = reembolsos.Count(r => r.Estado == "aprobado"),
                Pendientes = reembolsos.Count(r => r.Estado == "pendiente"),
                Rechazados = reembolsos.Count(r => r.Estado == "rechazado"),
                Lista = reembolsos.Take(50).ToList()
            };

            return View(vm);
        }

        // =========================================================
        // REPORTE: Ingresos Mensuales (Admin)
        // =========================================================
        [ValidarRol("Admin")]
        public ActionResult IngresosMensuales()
        {
            var pagos = db.pagos
                          .Where(p => p.Activo && p.Estado == "aprobado")
                          .ToList();

            var vm = pagos
                     .GroupBy(p => new { p.FechaPago.Year, p.FechaPago.Month })
                     .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                     .Select(g => new IngresoMensual
                     {
                         Anio = g.Key.Year,
                         Mes = g.Key.Month,
                         NombreMes = new System.Globalization.CultureInfo("es-ES")
                                        .DateTimeFormat.GetMonthName(g.Key.Month),
                         Total = g.Sum(x => x.Monto),
                         Cantidad = g.Count()
                     })
                     .ToList();

            return View(vm);
        }

        // =========================================================
        // REPORTE: Ventas por País (Admin) — basado en dirección del pago
        // =========================================================
        [ValidarRol("Admin")]
        public ActionResult VentasPais()
        {
            // Usamos el campo Direccion del pago como aproximación
            var pagos = db.pagos.Where(p => p.Activo && p.Estado == "aprobado").ToList();

            // Agrupamos simplificado (si quieres cruzar con la tabla paises puedes extenderlo)
            var vm = new VentasPaisViewModel
            {
                TotalPagos = pagos.Count,
                TotalMonto = pagos.Sum(p => p.Monto),
                Lista = pagos.Take(50).ToList()
            };

            return View(vm);
        }

        // =========================================================
        // DASHBOARD ORGANIZADOR
        // =========================================================
        [ValidarRol("Organizador")]
        public ActionResult ReporteOrganizador()
        {
            var vm = new DashboardOrganizadorViewModel
            {
                TotalTickets = db.compras.Where(c => c.Activo).Sum(c => (int?)c.Cantidad) ?? 0,
                TotalIngresos = db.pagos.Where(p => p.Activo && p.Estado == "aprobado").Sum(p => (decimal?)p.Monto) ?? 0,
                TotalEventos = db.tipos_ticket.Count(t => t.Activo)
            };
            return View(vm);
        }

        // =========================================================
        // REPORTE: Tickets Vendidos (Organizador)
        // =========================================================
        [ValidarRol("Organizador")]
        public ActionResult TicketsVendidos()
        {
            var compras = db.compras
                            .Where(c => c.Activo)
                            .OrderByDescending(c => c.FechaCompra)
                            .ToList();

            var tiposDict = db.tipos_ticket.ToList().ToDictionary(t => t.IdTipoTicket);

            var vm = new TicketsVendidosViewModel
            {
                TotalTickets = compras.Sum(c => c.Cantidad),
                TotalOrdenes = compras.Count,
                Detalle = compras.Take(50).Select(c => new TicketVendidoItem
                {
                    CodigoOrden = c.CodigoOrden,
                    NombreTicket = tiposDict.ContainsKey(c.IdTipoTicket) ? tiposDict[c.IdTipoTicket].Nombre : "—",
                    Cantidad = c.Cantidad,
                    PrecioUnit = c.PrecioUnitario,
                    Total = c.Total,
                    Estado = c.Estado,
                    Fecha = c.FechaCompra
                }).ToList()
            };

            return View(vm);
        }

        // =========================================================
        // REPORTE: Mis Ingresos (Organizador)
        // =========================================================
        [ValidarRol("Organizador")]
        public ActionResult MisIngresos()
        {
            var pagos = db.pagos
                          .Where(p => p.Activo && p.Estado == "aprobado")
                          .OrderByDescending(p => p.FechaPago)
                          .ToList();

            var vm = new MisIngresosViewModel
            {
                TotalIngresos = pagos.Sum(p => p.Monto),
                TotalPagos = pagos.Count,
                PorMes = pagos
                                   .GroupBy(p => new { p.FechaPago.Year, p.FechaPago.Month })
                                   .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                                   .Select(g => new IngresoMensual
                                   {
                                       Anio = g.Key.Year,
                                       Mes = g.Key.Month,
                                       NombreMes = new System.Globalization.CultureInfo("es-ES")
                                                      .DateTimeFormat.GetMonthName(g.Key.Month),
                                       Total = g.Sum(x => x.Monto),
                                       Cantidad = g.Count()
                                   }).ToList(),
                UltimosPagos = pagos.Take(20).ToList()
            };

            return View(vm);
        }

        // =========================================================
        // REPORTE: Eventos Populares (Organizador)
        // =========================================================
        [ValidarRol("Organizador")]
        public ActionResult EventosPopulares()
        {
            var datos = db.compras
                          .Where(c => c.Activo)
                          .GroupBy(c => c.IdTipoTicket)
                          .Select(g => new
                          {
                              IdTipo = g.Key,
                              Cantidad = g.Sum(x => x.Cantidad),
                              Ingresos = g.Sum(x => (decimal)x.Cantidad * x.PrecioUnitario)
                          })
                          .OrderByDescending(x => x.Cantidad)
                          .ToList();

            var tiposDict = db.tipos_ticket.ToList().ToDictionary(t => t.IdTipoTicket);

            var vm = datos.Select(d => new EventoVendidoResumen
            {
                NombreTicket = tiposDict.ContainsKey(d.IdTipo) ? tiposDict[d.IdTipo].Nombre : "Desconocido",
                Cantidad = d.Cantidad,
                Ingresos = d.Ingresos
            }).ToList();

            return View(vm);
        }

        // =========================================================
        // REPORTE: Ocupación del Venue (Organizador) — placeholder
        // =========================================================
        [ValidarRol("Organizador")]
        public ActionResult OcupacionVenue()
        {
            var tipos = db.tipos_ticket.Where(t => t.Activo).ToList();
            return View(tipos);
        }

        // =========================================================
        // REPORTE: Presentaciones por Evento (Organizador) — placeholder
        // =========================================================
        [ValidarRol("Organizador")]
        public ActionResult PresentacionesEvento()
        {
            var compras = db.compras
                            .Where(c => c.Activo)
                            .OrderByDescending(c => c.FechaCompra)
                            .Take(50)
                            .ToList();
            return View(compras);
        }

        // =========================================================
        // REPORTE: Uso de Descuentos (Organizador)
        // =========================================================
        [ValidarRol("Organizador")]
        public ActionResult UsoDescuentos()
        {
            var comprasConDesc = db.compras
                                   .Where(c => c.Activo && c.IdDescuento != null)
                                   .ToList();

            var vm = new UsoDescuentosViewModel
            {
                TotalConDescuento = comprasConDesc.Count,
                TotalTicketsDesc = comprasConDesc.Sum(c => c.Cantidad),
                PorDescuento = comprasConDesc
                                         .GroupBy(c => c.IdDescuento)
                                         .Select(g => new DescuentoResumen
                                         {
                                             IdDescuento = g.Key ?? 0,
                                             Usos = g.Count(),
                                             Tickets = g.Sum(x => x.Cantidad)
                                         }).ToList()
            };

            return View(vm);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
