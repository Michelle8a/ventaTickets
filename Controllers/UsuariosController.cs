using GestionTickets.Permisos;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace GestionTickets.Controllers
{
    [ValidarRol("Admin")]
    public class UsuariosController : Controller
    {
        private gestion_ticketsEntities db = new gestion_ticketsEntities();

        // GET: Usuarios
        public ActionResult Index()
        {
            var usuarios = db.usuarios
                .Include(u => u.paises)
                .Include(u => u.usuarios_rol.Select(ur => ur.roles))
                .OrderBy(u => u.nombre)
                .ToList();
            return View(usuarios);
        }

        // GET: Usuarios/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var usuario = db.usuarios
                .Include(u => u.paises)
                .Include(u => u.usuarios_rol.Select(ur => ur.roles))
                .FirstOrDefault(u => u.id_usuario == id);

            if (usuario == null)
                return HttpNotFound();

            return View(usuario);
        }

        // GET: Usuarios/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var usuario = db.usuarios
                .Include(u => u.usuarios_rol)
                .FirstOrDefault(u => u.id_usuario == id);

            if (usuario == null)
                return HttpNotFound();

            ViewBag.Roles = db.roles.Where(r => r.activo == true).ToList();
            ViewBag.RolActual = usuario.usuarios_rol.FirstOrDefault()?.id_rol;

            return View(usuario);
        }

        // POST: Usuarios/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id_usuario, string nombre, string apellido,
            string email, string telefono, bool activo, int? id_rol)
        {
            var usuario = db.usuarios
                .Include(u => u.usuarios_rol)
                .FirstOrDefault(u => u.id_usuario == id_usuario);

            if (usuario == null)
                return HttpNotFound();

            usuario.nombre = nombre;
            usuario.apellido = apellido;
            usuario.email = email;
            usuario.telefono = telefono;
            usuario.activo = activo;

            // Actualizar rol
            if (id_rol.HasValue)
            {
                var rolExistente = usuario.usuarios_rol.FirstOrDefault();
                if (rolExistente != null)
                {
                    rolExistente.id_rol = id_rol.Value;
                    db.Entry(rolExistente).State = EntityState.Modified;
                }
                else
                {
                    db.usuarios_rol.Add(new usuarios_rol
                    {
                        id_usuario = id_usuario,
                        id_rol = id_rol.Value
                    });
                }
            }

            db.Entry(usuario).State = EntityState.Modified;
            db.SaveChanges();

            return RedirectToAction("Index");
        }

        // GET: Usuarios/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var usuario = db.usuarios
                .Include(u => u.paises)
                .Include(u => u.usuarios_rol.Select(ur => ur.roles))
                .FirstOrDefault(u => u.id_usuario == id);

            if (usuario == null)
                return HttpNotFound();

            return View(usuario);
        }

        // POST: Usuarios/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var usuario = db.usuarios.Find(id);
            if (usuario == null)
                return HttpNotFound();

            usuario.activo = false;
            db.Entry(usuario).State = EntityState.Modified;
            db.SaveChanges();

            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                db.Dispose();
            base.Dispose(disposing);
        }
    }
}