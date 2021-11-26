using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ComprasCartonesLGP.Dal;
using ComprasCartonesLGP.Entities;
using ComprasCartonesLGP.Utilities;
using ExcelDataReader;
using PagedList;

namespace ComprasCartonesLGP.Web.Areas.ContentAdmin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class SolicitudesController : Controller
    {
        private LGPContext db = new LGPContext();

        // GET: ContentAdmin/Solicitudes
        public ActionResult Index(string searchString, string currentFilter, int? page)
        {
            if (!string.IsNullOrEmpty(searchString))
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }
            ViewBag.page = page;
            ViewBag.CurrentFilter = searchString;

            int pageSize = 30;
            int pageNumber = (page ?? 1);

            //var solicitudes = db.Solicitudes.Include(s => s.Promocion);
            var solicitudes = db.Solicitudes.OrderByDescending(x => x.Promocion.Anio).ToList();
            if (!string.IsNullOrEmpty(searchString))
            {
                solicitudes = solicitudes.Where(x => x.NroSolicitud.ToUpper().Contains(searchString.ToUpper())).ToList();
            }

            return View(solicitudes.ToPagedList(pageNumber, pageSize));
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

        public ActionResult Importar()
        {
            ViewBag.PromocionId = new SelectList(db.Promociones, "ID", "Descripcion");
            return View();
        }

        [HttpPost]
        public ActionResult Importar(HttpPostedFileBase file, int? PromocionId, float Precio)
        {

            List<Solicitud> solicitudes = new List<Solicitud>();
            ViewBag.PromocionId = new SelectList(db.Promociones, "ID", "Descripcion");
            List<Alert> alerts = new List<Alert>();

            if (file.ContentLength > 0)
            {
                string extension = Path.GetExtension(file.FileName).ToLower();

                string[] validFileTypes = { ".xls", ".xlsx", ".csv" };

                string path1 = string.Format("{0}/{1}", Server.MapPath("~/Areas/Data/Uploads"), file.FileName);
                if (!Directory.Exists(path1))
                {
                    Directory.CreateDirectory(Server.MapPath("~/Areas/Data/Uploads"));
                }

                file.SaveAs(path1);

                List<string> data = new List<string>();

                using (var stream = System.IO.File.Open(path1, FileMode.Open, FileAccess.Read))
                {
                    using (var reader = ExcelReaderFactory.CreateReader(stream))
                    {
                        var result = reader.AsDataSet();
                        // Ejemplos de acceso a datos
                        DataTable table = result.Tables[0];
                        for (int i = 1; i <= (table.Rows.Count - 1); i++)
                        {
                            DataRow rows = table.Rows[i];
                            string dataRow = "";
                            for (int j = 0; j <= (table.Columns.Count - 1); j++)
                            {
                                dataRow += rows[j] + ((j != table.Columns.Count) ? ";" : "");
                            }
                            data.Add(dataRow);
                        }
                    }
                }

                foreach (var row in data)
                {
                    string[] solicitudAuxiliar = row.Split(';');
                    if (solicitudAuxiliar.Length == 2)
                    {
                        Solicitud solicitudNueva = new Solicitud();
                        if (!int.TryParse(solicitudAuxiliar[0].Trim(), out int nroSolicitud))
                        {
                            string errorMessage = "Error en Linea: " + (data.IndexOf(row) + 2 ) + ", El numero de solicitud no es valido";
                            alerts.Add(new Alert("danger", errorMessage, true));
                            continue;
                        }
                        solicitudNueva.NroSolicitud = nroSolicitud.ToString();
                        solicitudNueva.Precio = Precio;
                        solicitudNueva.PromocionId = Convert.ToInt32(PromocionId);

                        if (db.Solicitudes.Where(x => x.PromocionId == solicitudNueva.PromocionId && x.NroSolicitud == solicitudNueva.NroSolicitud).FirstOrDefault() == null)
                        {
                            solicitudes.Add(solicitudNueva);
                            //db.Solicitudes.Add(solicitudNueva);
                            //db.SaveChanges();
                        }
                    }
                    else
                    {
                        string errorMessage = "Error en Linea: " + data.IndexOf(row) + ", Formato no Valido.";
                        alerts.Add(new Alert("danger", errorMessage, true));
                        continue;
                    }
                }

                if (alerts.Count == 0)
                {
                    db.Solicitudes.AddRange(solicitudes);

                    db.SaveChanges();

                    return RedirectToAction("ImportadoExitoso");
                }

                ViewBag.Alerts = alerts;
            }
            return View();

        }

        public ActionResult ImportadoExitoso()
        {
            return View();
        }
    }
}
