using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace GestionTickets.Controllers
{
    public class TiposTicketController : Controller
    {
        // GET: TiposTicket
        public ActionResult Index()
        {
            return View();
        }
    }
}