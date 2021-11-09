using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ComprasCartonesLGP.Web.Areas.ContentAdmin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class HomeController : Controller
    {
        // GET: ContentAdmin/Home
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult HabilitarCompra()
        {
            return View();
        }
    }
}