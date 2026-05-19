using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GestionTickets.Models
{
    [Table("tipos_ticket")]
    public class TipoTicket
    {
        [Key]
        [Column("id_tipo_ticket")]
        public int IdTipoTicket { get; set; }

        [Column("id_evento")]
        public int IdEvento { get; set; }

        [Column("id_seccion")]
        public int IdSeccion { get; set; }

        [Column("nombre")]
        [StringLength(100)]
        public string Nombre { get; set; }

        [Column("precio")]
        public decimal Precio { get; set; }

        [Column("moneda")]
        [StringLength(5)]
        public string Moneda { get; set; }

        [Column("cantidad_ticket")]
        public int CantidadTicket { get; set; }

        [Column("venta_inicio")]
        public DateTime VentaInicio { get; set; }

        [Column("venta_fin")]
        public DateTime VentaFin { get; set; }

        [Column("activo")]
        public bool Activo { get; set; }
    }
}