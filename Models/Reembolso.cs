using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GestionTickets.Models
{
    [Table("reembolsos")]
    public class Reembolso
    {
        [Key]
        [Column("id_reembolso")]
        public int IdReembolso { get; set; }

        [Column("id_compra")]
        public int IdCompra { get; set; }

        [Column("id_pago")]
        public int IdPago { get; set; }

        [Required(ErrorMessage = "El monto es obligatorio.")]
        [Column("monto")]
        [Display(Name = "Monto a reembolsar")]
        public decimal Monto { get; set; }

        [Required(ErrorMessage = "El motivo es obligatorio.")]
        [Column("motivo")]
        [StringLength(500)]
        [Display(Name = "Motivo del reembolso")]
        public string Motivo { get; set; }

        [Column("estado")]
        [StringLength(20)]
        [Display(Name = "Estado")]
        public string Estado { get; set; } = "pendiente";

        [Column("fecha_solicitud")]
        [Display(Name = "Fecha de solicitud")]
        public DateTime FechaSolicitud { get; set; } = DateTime.Now;

        [Column("fecha_proceso")]
        [Display(Name = "Fecha de proceso")]
        public DateTime? FechaProceso { get; set; }

        [Column("stripe_refund_id")]
        [StringLength(100)]
        public string StripeRefundId { get; set; }

        // Datos de navegación para las vistas (no persistidos)
        [NotMapped]
        public Pago Pago { get; set; }

        [NotMapped]
        public Compra Compra { get; set; }
    }
}
