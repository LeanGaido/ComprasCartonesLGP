using ComprasCartonesLGP.Dal;
using ComprasCartonesLGP.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace ComprasCartonesLGP.Web.Areas.ContentAdmin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ComprasController : Controller
    {
        private LGPContext db = new LGPContext();
        // GET: ContentAdmin/Compras
        public ActionResult Index()
        {
            var compras = db.ComprasDeSolicitudes.OrderByDescending(x => x.ID).ToList();
            return View(compras);
        }

        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CompraDeSolicitud compra = db.ComprasDeSolicitudes.Find(id);
            if (compra == null)
            {
                return HttpNotFound();
            }
            var asociado = db.Asociados.Where(x => x.ID == compra.AsociadoID).FirstOrDefault();
            if(compra.PagoRealizdo == true)
            {
                ViewBag.EstadoPago = "Completo"; 
            }
            else
            {
                ViewBag.EstadoPago = "Pendiente";
            }

            ViewBag.NombreAsociado = asociado.NombreCompleto;
            return View(compra);
        }
    }
}