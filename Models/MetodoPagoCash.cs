using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GestionTickets.Models
{
    [Table("metodos_pago_cash")]
    public class MetodoPagoCash
    {
        [Key]
        [Column("id_metodo_cash")]
        public int IdMetodoCash { get; set; }

        [Column("punto_pago")]
        [StringLength(150)]
        public string PuntoPago { get; set; }

        [Column("codigo_referencia")]
        [StringLength(100)]
        public string CodigoReferencia { get; set; }

        [Column("activo")]
        public bool Activo { get; set; } = true;
    }
}