using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GestionTickets.Models
{
    [Table("pagos")]
    public class Pago
    {
        [Key]
        [Column("id_pago")]
        public int IdPago { get; set; }

        [Column("id_compra")]
        public int IdCompra { get; set; }

        [Column("id_usuario")]
        public int IdUsuario { get; set; }

        [Required(ErrorMessage = "Selecciona un método de pago.")]
        [Column("metodo")]
        [StringLength(20)]
        [Display(Name = "Método de pago")]
        public string Metodo { get; set; }

        [Column("id_metodo_tarjeta")]
        public int? IdMetodoTarjeta { get; set; }

        [Column("id_metodo_transferencia")]
        public int? IdMetodoTransferencia { get; set; }

        [Column("id_metodo_cash")]
        public int? IdMetodoCash { get; set; }

        [Column("monto")]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public decimal Monto { get; set; }

        [Column("moneda")]
        [StringLength(5)]
        public string Moneda { get; set; } = "USD";

        [Column("estado")]
        [StringLength(20)]
        public string Estado { get; set; } = "pendiente";

        [Column("fecha_pago")]
        public DateTime FechaPago { get; set; } = DateTime.Now;

        [Column("activo")]
        public bool Activo { get; set; } = true;

        // ─── Campos de FORMULARIO (no se guardan) ────────────
        [NotMapped] public string TitularTarjeta { get; set; }
        [NotMapped] public string NumeroTarjeta { get; set; }
        [NotMapped] public string Vencimiento { get; set; }
        [NotMapped] public string Cvv { get; set; }
        [NotMapped] public decimal? PagaCon { get; set; }
        [NotMapped] public string Nombre { get; set; }
        [NotMapped] public string Telefono { get; set; }
        [NotMapped] public string Direccion { get; set; }
        [NotMapped] public string Notas { get; set; }

        // ─── Resumen del pedido ──────────────────────────────
        [NotMapped] public int Cantidad { get; set; }
        [NotMapped] public decimal Subtotal { get; set; }
        [NotMapped] public decimal Cargos { get; set; }
        [NotMapped] public decimal Envio { get; set; } = 1.00m;
        [NotMapped] public decimal Propina { get; set; }

        [NotMapped]
        public decimal Total => Subtotal + Cargos + Envio + Propina;

        // ─── Alias para compatibilidad ───────────────────────
        [NotMapped]
        public string MetodoPago
        {
            get => Metodo;
            set => Metodo = value;
        }

        // Alias para CompraId (usado en algunas vistas)
        [NotMapped]
        public int CompraId
        {
            get => IdCompra;
            set => IdCompra = value;
        }
    }
}