using ComprasCartonesLGP.Dal;
using ComprasCartonesLGP.Entities;
using ComprasCartonesLGP.Utilities;
using ExcelDataReader;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ComprasCartonesLGP.Web.Areas.ContentAdmin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ImportarController : Controller
    {
        private LGPContext db = new LGPContext();

        // GET: ContentAdmin/Importar
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult ImportarLocalidades()
        {
            ViewBag.PromocionId = new SelectList(db.Promociones, "ID", "Descripcion");
            return View();
        }

        [HttpPost]
        public ActionResult ImportarLocalidades(HttpPostedFileBase file)
        {

            List<Localidades> localidades = new List<Localidades>();
            List<Alert> alerts = new List<Alert>();

            if (file.ContentLength > 0)
            {
                string extension = Path.GetExtension(file.FileName).ToLower();

                string[] validFileTypes = { ".xls", ".xlsx", ".csv" };

                string path1 = string.Format("{0}/{1}", Server.MapPath("~/Areas/ContentAdmin/Data/Uploads"), file.FileName);
                if (!Directory.Exists(path1))
                {
                    Directory.CreateDirectory(Server.MapPath("~/Areas/ContentAdmin/Data/Uploads"));
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
                    string[] localidadAuxiliar = row.Split(';');
                    if (localidadAuxiliar.Length == 4)
                    {
                        Localidades localidadNueva = new Localidades();
                        if (!int.TryParse(localidadAuxiliar[0].Trim(), out int ID))
                        {
                            string errorMessage = "Error en Linea: " + (data.IndexOf(row) + 2) + ", El numero de Id no es valido";
                            alerts.Add(new Alert("danger", errorMessage, true));
                            continue;
                        }

                        if (string.IsNullOrEmpty(localidadAuxiliar[1].Trim()))
                        {
                            string errorMessage = "Error en Linea: " + (data.IndexOf(row) + 2) + ", El nombre de localidad no puede estar vacio";
                            alerts.Add(new Alert("danger", errorMessage, true));
                            continue;
                        }

                        if (!int.TryParse(localidadAuxiliar[2].Trim(), out int idPcia))
                        {
                            string errorMessage = "Error en Linea: " + (data.IndexOf(row) + 2) + ", El numero de Id de provincia no es valido";
                            alerts.Add(new Alert("danger", errorMessage, true));
                            continue;
                        }

                        localidadNueva.ID = Convert.ToInt32(localidadAuxiliar[0].Trim());
                        localidadNueva.Descripcion = localidadAuxiliar[1].Trim();
                        localidadNueva.ProvinciaID = Convert.ToInt32(localidadAuxiliar[2].Trim());

                        var localidadExistente = db.Localidades.Where(x => x.ID == localidadNueva.ID).FirstOrDefault();

                        if (localidadExistente == null)
                        {
                            localidades.Add(localidadNueva);
                        }
                        else
                        {
                            if(localidadExistente.Descripcion.Trim() != localidadNueva.Descripcion)
                            {
                                localidadExistente.Descripcion = localidadNueva.Descripcion;
                                db.Entry(localidadExistente).State = EntityState.Modified;
                                db.SaveChanges();
                            }
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
                    db.Localidades.AddRange(localidades);

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