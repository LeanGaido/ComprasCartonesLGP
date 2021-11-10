using ComprasCartonesLGP.Dal;
using ComprasCartonesLGP.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ComprasCartonesLGP.Web.Areas.ContentAdmin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class HomeController : Controller
    {
        private LGPContext db = new LGPContext();
        // GET: ContentAdmin/Home
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult HabilitarCompra()
        {
            var habilitacion = db.Parametros.Where(x => x.Clave == "HabilitarBoton").FirstOrDefault();
            ViewBag.Estado = habilitacion.Valor;
            return View();
        }

        [HttpPost]
        public ActionResult HabilitarCompra(string habilitado, string fechaHabilitacion)
        {
            var habilitacion = db.Parametros.Where(x => x.Clave == "HabilitarBoton").FirstOrDefault();
            ViewBag.Estado = habilitacion.Valor;
            if (habilitado == "false")
            {
                if (fechaHabilitacion == "")
                {
                    ViewBag.Error = "En caso de deshabilitar la compra, informe en que fecha se volvera a habilitar.";
                    return View();
                }
                var fecha = db.Parametros.Where(x => x.Clave == "FechaHabilitacion").FirstOrDefault();
                fecha.Valor = fechaHabilitacion;
                db.Entry(fecha).State = EntityState.Modified;
            }
            habilitacion.Valor = habilitado;
            db.Entry(habilitacion).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("CompraHabilitadaExitosamente");
        }

        public ActionResult CompraHabilitadaExitosamente()
        {
            return View();
        }
    }
}