using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web.Mvc;

namespace GestionTickets.Controllers
{
    public class AccesoController : Controller
    {
        private gestion_ticketsEntities db = new gestion_ticketsEntities();

        // ============================
        // VISTAS
        // ============================
        public ActionResult Login()
        {
            return View();
        }

        public ActionResult Registrar()
        {
            ViewBag.Paises = db.paises.ToList(); // para dropdown
            return View();
        }

        // ============================
        // REGISTRAR USUARIO (AJAX)
        // ============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult Registrar(string nombre, string apellido, string email, string password, string telefono, int id_pais)
        {
            try
            {
                var existe = db.usuarios.Any(u => u.email == email);

                if (existe)
                {
                    return Json(new { success = false, mensaje = "El correo ya está registrado" });
                }

                string hash = ConvertirSha256(password);

                usuarios nuevo = new usuarios()
                {
                    nombre = nombre,
                    apellido = apellido,
                    email = email,
                    telefono = telefono,
                    password_hash = hash,
                    id_pais = id_pais,
                    fecha_registro = DateTime.Now,
                    activo = true
                };

                db.usuarios.Add(nuevo);
                db.SaveChanges();

                var rolUsuario = db.roles.FirstOrDefault(r => r.nombre == "Usuario");

                if (rolUsuario != null)
                {
                    usuarios_rol ur = new usuarios_rol()
                    {
                        id_usuario = nuevo.id_usuario,
                        id_rol = rolUsuario.id_rol
                    };

                    db.usuarios_rol.Add(ur);
                    db.SaveChanges();
                }

                return Json(new { success = true, mensaje = "Usuario registrado correctamente" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, mensaje = ex.Message });
            }
        }

        // ============================
        // LOGIN (AJAX)
        // ============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult Login(string email, string password)
        {
            string hash = ConvertirSha256(password);

            var usuario = db.usuarios
                .FirstOrDefault(u => u.email == email && u.password_hash == hash && u.activo == true);

            if (usuario != null)
            {
                Session["usuario"] = usuario;
                return Json(new { success = true });
            }
            else
            {
                return Json(new { success = false, mensaje = "Credenciales incorrectas" });
            }
        }

        // ============================
        // LOGOUT
        // ============================
        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Login");
        }

        // ============================
        // HASH SHA256
        // ============================
        public static string ConvertirSha256(string texto)
        {
            StringBuilder sb = new StringBuilder();

            using (SHA256 hash = SHA256.Create())
            {
                byte[] result = hash.ComputeHash(Encoding.UTF8.GetBytes(texto));

                foreach (byte b in result)
                {
                    sb.Append(b.ToString("x2"));
                }
            }

            return sb.ToString();
        }
    }
}