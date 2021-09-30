using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ComprasCartonesLGP.Dal;
using ComprasCartonesLGP.Entities;

namespace ComprasCartonesLGP.Web.Areas.ContentAdmin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class SolicitudesController : Controller
    {
        private LGPContext db = new LGPContext();

        // GET: ContentAdmin/Solicitudes
        public ActionResult Index()
        {
            var solicitudes = db.Solicitudes.Include(s => s.Promocion);
            return View(solicitudes.ToList());
        }

        // GET: ContentAdmin/Solicitudes/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Solicitud solicitud = db.Solicitudes.Find(id);
            if (solicitud == null)
            {
                return HttpNotFound();
            }
            return View(solicitud);
        }

        // GET: ContentAdmin/Solicitudes/Create
        public ActionResult Create()
        {
            ViewBag.PromocionId = new SelectList(db.Promociones, "ID", "Descripcion");
            return View();
        }

        // POST: ContentAdmin/Solicitudes/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,PromocionId,NroSolicitud,NroCarton,Precio")] Solicitud solicitud)
        {
            if (ModelState.IsValid)
            {
                db.Solicitudes.Add(solicitud);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.PromocionId = new SelectList(db.Promociones, "ID", "Descripcion", solicitud.PromocionId);
            return View(solicitud);
        }

        // GET: ContentAdmin/Solicitudes/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Solicitud solicitud = db.Solicitudes.Find(id);
            if (solicitud == null)
            {
                return HttpNotFound();
            }
            ViewBag.PromocionId = new SelectList(db.Promociones, "ID", "Descripcion", solicitud.PromocionId);
            return View(solicitud);
        }

        // POST: ContentAdmin/Solicitudes/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,PromocionId,NroSolicitud,NroCarton,Precio")] Solicitud solicitud)
        {
            if (ModelState.IsValid)
            {
                db.Entry(solicitud).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.PromocionId = new SelectList(db.Promociones, "ID", "Descripcion", solicitud.PromocionId);
            return View(solicitud);
        }

        // GET: ContentAdmin/Solicitudes/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Solicitud solicitud = db.Solicitudes.Find(id);
            if (solicitud == null)
            {
                return HttpNotFound();
            }
            return View(solicitud);
        }

        // POST: ContentAdmin/Solicitudes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Solicitud solicitud = db.Solicitudes.Find(id);
            db.Solicitudes.Remove(solicitud);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
