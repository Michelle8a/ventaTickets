using System.Data.Entity;

namespace GestionTickets.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext() : base("ApplicationDbContext")
        {
        }

        public DbSet<Compra>                   compras                    { get; set; }
        public DbSet<Pago>                     pagos                      { get; set; }
        public DbSet<TipoTicket>               tipos_ticket               { get; set; }
        public DbSet<MetodoPagoTarjeta>        metodos_pago_tarjeta       { get; set; }
        public DbSet<MetodoPagoCash>           metodos_pago_cash          { get; set; }
        public DbSet<MetodoPagoTransferencia>  metodos_pago_transferencia { get; set; }
        public DbSet<Reembolso>                reembolsos                 { get; set; }
    }
}
