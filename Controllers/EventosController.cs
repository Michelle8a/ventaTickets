using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace GestionTickets.Controllers
{
    public class EventosController : Controller
    {
        private gestion_ticketsEntities db = new gestion_ticketsEntities();
        
        // GET: Eventos
        public ActionResult Index()
        {
            var eventos = db.sp_mostrar_eventos(null).ToList();
            return View(eventos);
        }
    }
}