using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace GestionTickets.Models
{
    public class Compra
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "La cantidad es obligatoria")]
        [Range(1, 4, ErrorMessage = "Solo puedes comprar entre 1 y 4 boletos")]
        public int Cantidad { get; set; }

        [Required(ErrorMessage = "Seleccione un método de pago")]
        public string MetodoPago { get; set; }

        [StringLength(16, MinimumLength = 13, ErrorMessage = "Tarjeta inválida")]
        public string NumeroTarjeta { get; set; }

        public decimal PrecioUnitario { get; set; } = 10;
        public decimal CargoServicio { get; set; } = 2;

        public decimal Subtotal { get; set; }
        public decimal Cargos { get; set; }
        public decimal Total { get; set; }

        public DateTime Fecha { get; set; } = DateTime.Now;
    }
}
