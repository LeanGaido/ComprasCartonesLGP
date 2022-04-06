using ComprasCartonesLGP.Dal;
using ComprasCartonesLGP.Entities;
using ExcelDataReader;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ComprasCartonesLGP.Web.Controllers
{
    public class HomeController : Controller
    {
        private LGPContext db = new LGPContext();
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        //public void Importar()
        //{
        //    List<Asociado> Usuarios = new List<Asociado>();

        //    string[] validFileTypes = { ".xls", ".xlsx", ".csv" };

        //    string path1 = string.Format("{0}", Server.MapPath("~/Content/ExcelAsociados/Asociados.xlsx"));

        //    List<string> data = new List<string>();

        //    using (var stream = System.IO.File.Open(path1, FileMode.Open, FileAccess.Read))
        //    {
        //        using (var reader = ExcelReaderFactory.CreateReader(stream))
        //        {
        //            var result = reader.AsDataSet();
        //            // Ejemplos de acceso a datos
        //            DataTable table = result.Tables[0];
        //            for (int i = 1; i <= (table.Rows.Count - 1); i++)
        //            {
        //                DataRow rows = table.Rows[i];
        //                string dataRow = "";
        //                for (int j = 0; j <= (table.Columns.Count - 1); j++)
        //                {
        //                    dataRow += rows[j] + ((j != table.Columns.Count) ? ";" : "");
        //                }
        //                data.Add(dataRow);
        //            }
        //        }
        //    }
        //    foreach (var row in data)
        //    {
        //        try
        //        {
        //            string[] usuarioAux = row.Split(';');
        //            if (usuarioAux.Length == 18)
        //            {
        //                Asociado usuario = new Asociado();
        //                var IdUsuario = Convert.ToInt32(usuarioAux[15].Trim());
        //                usuario = db.Asociados.Where(x => x.ID == IdUsuario).FirstOrDefault();
        //                usuario.LocalidadID = Convert.ToInt32(usuarioAux[16].Trim());
        //                db.Entry(usuario).State = EntityState.Modified;
        //                db.SaveChanges();
        //            }
        //        }
        //        catch (Exception e)
        //        {
        //            var ruta = System.Web.Hosting.HostingEnvironment.MapPath("~/Content/ExcelAsociados/UsuariosNoGuardados.xml");

        //            using (TextWriter tw = new StreamWriter(ruta, true))
        //            {
        //                tw.WriteLine("Error: " + e.Message + "Linea: " + row);
        //            }
        //            continue;
        //        }

        //    }

        //}
    }
}