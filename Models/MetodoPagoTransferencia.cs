using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GestionTickets.Models
{
    [Table("metodos_pago_transferencia")]
    public class MetodoPagoTransferencia
    {
        [Key]
        [Column("id_metodo_transferencia")]
        public int IdMetodoTransferencia { get; set; }

        [Column("banco")]
        [StringLength(100)]
        public string Banco { get; set; }

        [Column("numero_cuenta")]
        [StringLength(50)]
        public string NumeroCuenta { get; set; }

        [Column("titular_cuenta")]
        [StringLength(150)]
        public string TitularCuenta { get; set; }

        [Column("referencia")]
        [StringLength(100)]
        public string Referencia { get; set; }

        [Column("activo")]
        public bool Activo { get; set; } = true;
    }
}