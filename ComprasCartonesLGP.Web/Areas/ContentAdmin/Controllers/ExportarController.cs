using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ComprasCartonesLGP.Web.Areas.ContentAdmin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ExportarController : Controller
    {
        // GET: ContentAdmin/Exportar
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult ExportarClientes()
        {
            return View();
        }

        //public FileContentResult ExportarExcelClientes()
        //{

        //}
    }
}