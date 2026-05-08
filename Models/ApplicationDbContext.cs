using System.Data.Entity;

namespace GestionTickets.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext() : base("gestion_ticketsEntities")
        {
        }
        public DbSet<Compra> compras { get; set; }
    }
}