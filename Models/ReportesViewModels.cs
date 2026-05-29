using GestionTickets.Models;
using System;
using System.Collections.Generic;

namespace GestionTickets.Models
{
    // ── Admin Dashboard KPIs ──────────────────────────────────────
    public class DashboardAdminViewModel
    {
        public decimal TotalVentas { get; set; }
        public int TotalTickets { get; set; }
        public int TotalPagos { get; set; }
        public int TotalReembolsos { get; set; }
    }

    // ── Organizador Dashboard KPIs ────────────────────────────────
    public class DashboardOrganizadorViewModel
    {
        public int TotalTickets { get; set; }
        public decimal TotalIngresos { get; set; }
        public int TotalEventos { get; set; }
    }

    // ── Reporte: Ventas Totales ───────────────────────────────────
    public class VentasTotalesViewModel
    {
        public decimal TotalIngresos { get; set; }
        public int TotalPagos { get; set; }
        public List<MetodoResumen> PorMetodo { get; set; }
        public List<Pago> UltimosPagos { get; set; }
    }

    // ── Reporte: Métodos de Pago ──────────────────────────────────
    public class MetodoResumen
    {
        public string Metodo { get; set; }
        public decimal Total { get; set; }
        public int Cantidad { get; set; }
    }

    // ── Reporte: Eventos Vendidos / Populares ─────────────────────
    public class EventoVendidoResumen
    {
        public string NombreTicket { get; set; }
        public int Cantidad { get; set; }
        public decimal Ingresos { get; set; }
    }

    // ── Reporte: Reembolsos ───────────────────────────────────────
    public class ReembolsosReporteViewModel
    {
        public decimal TotalSolicitado { get; set; }
        public int Aprobados { get; set; }
        public int Pendientes { get; set; }
        public int Rechazados { get; set; }
        public List<Reembolso> Lista { get; set; }
    }

    // ── Reporte: Ingresos Mensuales ───────────────────────────────
    public class IngresoMensual
    {
        public int Anio { get; set; }
        public int Mes { get; set; }
        public string NombreMes { get; set; }
        public decimal Total { get; set; }
        public int Cantidad { get; set; }
    }

    // ── Reporte: Ventas por País ──────────────────────────────────
    public class VentasPaisViewModel
    {
        public int TotalPagos { get; set; }
        public decimal TotalMonto { get; set; }
        public List<Pago> Lista { get; set; }
    }

    // ── Reporte: Tickets Vendidos (Organizador) ───────────────────
    public class TicketsVendidosViewModel
    {
        public int TotalTickets { get; set; }
        public int TotalOrdenes { get; set; }
        public List<TicketVendidoItem> Detalle { get; set; }
    }

    public class TicketVendidoItem
    {
        public string CodigoOrden { get; set; }
        public string NombreTicket { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnit { get; set; }
        public decimal Total { get; set; }
        public string Estado { get; set; }
        public DateTime Fecha { get; set; }
    }

    // ── Reporte: Mis Ingresos (Organizador) ──────────────────────
    public class MisIngresosViewModel
    {
        public decimal TotalIngresos { get; set; }
        public int TotalPagos { get; set; }
        public List<IngresoMensual> PorMes { get; set; }
        public List<Pago> UltimosPagos { get; set; }
    }

    // ── Reporte: Uso de Descuentos (Organizador) ──────────────────
    public class UsoDescuentosViewModel
    {
        public int TotalConDescuento { get; set; }
        public int TotalTicketsDesc { get; set; }
        public List<DescuentoResumen> PorDescuento { get; set; }
    }

    public class DescuentoResumen
    {
        public int IdDescuento { get; set; }
        public int Usos { get; set; }
        public int Tickets { get; set; }
    }
}
