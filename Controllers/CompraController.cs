using GestionTickets.Models;
using GestionTickets.Permisos;
using System;
using System.Linq;
using System.Web.Mvc;

namespace GestionTickets.Controllers
{
    [ValidarSesion]
    public class CompraController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // LISTADO
        public ActionResult Index()
        {
            return View(db.compras.OrderByDescending(c => c.FechaCompra).ToList());
        }

        // FORM: Compra/Create?idTipoTicket=5
        public ActionResult Create(int? idTipoTicket)
        {
            TipoTicket tipo = null;

            if (idTipoTicket.HasValue)
            {
                tipo = db.tipos_ticket.Find(idTipoTicket.Value);
            }

            // Si no llegó parámetro o no existe, buscar uno activo y vigente
            if (tipo == null)
            {
                var ahora = DateTime.Now;
                tipo = db.tipos_ticket
                    .Where(t => t.Activo && t.VentaInicio <= ahora && t.VentaFin >= ahora)
                    .OrderBy(t => t.IdTipoTicket)
                    .FirstOrDefault();

                // Si tampoco hay vigente, tomar cualquier activo
                if (tipo == null)
                {
                    tipo = db.tipos_ticket
                        .Where(t => t.Activo)
                        .OrderBy(t => t.IdTipoTicket)
                        .FirstOrDefault();
                }
            }

            if (tipo == null)
            {
                ModelState.AddModelError("", "No hay tipos de ticket disponibles.");
                return View(new Compra());
            }

            var compra = new Compra
            {
                IdTipoTicket = tipo.IdTipoTicket,
                Cantidad = 1,
                PrecioUnitario = tipo.Precio
            };

            // Pasar info del tipo a la vista
            ViewBag.NombreTicket = tipo.Nombre;
            ViewBag.Moneda = tipo.Moneda;
            ViewBag.CantidadDisponible = tipo.CantidadTicket;

            return View(compra);
        }

        // POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Compra compra)
        {
            ModelState.Remove("MetodoPago");
            ModelState.Remove("NumeroTarjeta");

            if (compra.Cantidad > 4)
            {
                ModelState.AddModelError("", "Máximo 4 boletos permitidos.");
            }

            // Validar tipo de ticket
            var tipo = db.tipos_ticket.Find(compra.IdTipoTicket);
            if (tipo == null)
            {
                ModelState.AddModelError("", "El tipo de ticket no existe.");
                return View(compra);
            }

            // Validar disponibilidad
            if (compra.Cantidad > tipo.CantidadTicket)
            {
                ModelState.AddModelError("", $"Solo quedan {tipo.CantidadTicket} tickets disponibles.");
            }

            // Validar usuario logueado
            if (compra.IdUsuario == 0)
            {
                // TODO: usar Session["IdUsuario"] cuando esté implementado
                var primerUsuario = db.Database
                    .SqlQuery<int>("SELECT TOP 1 id_usuario FROM usuarios WHERE activo = 1 ORDER BY id_usuario")
                    .FirstOrDefault();

                if (primerUsuario == 0)
                {
                    ModelState.AddModelError("", "No hay usuarios activos.");
                    return View(compra);
                }
                compra.IdUsuario = primerUsuario;
            }

            if (ModelState.IsValid)
            {
                // Forzar el precio desde la BD (seguridad: nunca confiar en el cliente)
                compra.PrecioUnitario = tipo.Precio;
                compra.CodigoOrden = $"ORD-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 4).ToUpper()}";
                compra.Estado = "pendiente";
                compra.FechaCompra = DateTime.Now;
                compra.Activo = true;
                // NO asignar compra.Total — es columna calculada

                db.compras.Add(compra);
                db.SaveChanges();

                return RedirectToAction("Create", "Pagos", new { compraId = compra.Id });
            }

            // Si falla, rehidratar info para la vista
            ViewBag.NombreTicket = tipo.Nombre;
            ViewBag.Moneda = tipo.Moneda;
            ViewBag.CantidadDisponible = tipo.CantidadTicket;
            return View(compra);
        }

        // DETALLE
        public ActionResult Detalle(int id)
        {
            var compra = db.compras.Find(id);
            if (compra == null) return HttpNotFound();

            // Hidratar campos calculados para la vista
            compra.Subtotal = compra.Cantidad * compra.PrecioUnitario;
            compra.Cargos = compra.Cantidad * compra.CargoServicio;

            // Cargar nombre del tipo de ticket
            var tipo = db.tipos_ticket.Find(compra.IdTipoTicket);
            ViewBag.NombreTicket = tipo?.Nombre ?? "Ticket";

            return View(compra);
        }
    }
}