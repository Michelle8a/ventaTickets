using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GestionTickets.Models
{
    [Table("metodos_pago_tarjeta")]
    public class MetodoPagoTarjeta
    {
        [Key]
        [Column("id_metodo_tarjeta")]
        public int IdMetodoTarjeta { get; set; }

        [Column("titular")]
        [StringLength(150)]
        public string Titular { get; set; }

        [Column("numero_enmascarado")]
        [StringLength(20)]
        public string NumeroEnmascarado { get; set; }

        [Column("tipo_tarjeta")]
        [StringLength(30)]
        public string TipoTarjeta { get; set; }

        [Column("fecha_expiracion")]
        [StringLength(7)]
        public string FechaExpiracion { get; set; }

        [Column("activo")]
        public bool Activo { get; set; } = true;
    }
}