using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GestionTickets.Models
{
    [Table("compras")]
    public class Compra
    {
        [Key]
        [Column("id_compra")]
        public int Id { get; set; }

        [Column("id_usuario")]
        public int IdUsuario { get; set; }

        [Column("id_tipo_ticket")]
        public int IdTipoTicket { get; set; }

        [Column("id_descuento")]
        public int? IdDescuento { get; set; }

        [Column("codigo_orden")]
        [StringLength(50)]
        public string CodigoOrden { get; set; }

        [Required(ErrorMessage = "La cantidad es obligatoria")]
        [Range(1, 4, ErrorMessage = "Solo puedes comprar entre 1 y 4 boletos")]
        [Column("cantidad")]
        public int Cantidad { get; set; }

        [Column("precio_unitario")]
        public decimal PrecioUnitario { get; set; } = 10m;

        [Column("total")]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public decimal Total { get; set; }

        [Column("estado")]
        [StringLength(20)]
        public string Estado { get; set; } = "pendiente";

        [Column("fecha_compra")]
        public DateTime FechaCompra { get; set; } = DateTime.Now;

        [Column("activo")]
        public bool Activo { get; set; } = true;

        // ─── Propiedades calculadas (no van a BD) ─────────────
        [NotMapped]
        public decimal CargoServicio { get; set; } = 2m;

        // ✅ CAMBIADAS A PROPIEDADES NORMALES (read/write) para el controlador
        [NotMapped]
        public decimal Subtotal { get; set; }

        [NotMapped]
        public decimal Cargos { get; set; }

        // Alias para vistas viejas
        [NotMapped]
        public DateTime Fecha
        {
            get => FechaCompra;
            set => FechaCompra = value;
        }

        [NotMapped]
        public string MetodoPago { get; set; }

        [NotMapped]
        public string NumeroTarjeta { get; set; }
    }
}