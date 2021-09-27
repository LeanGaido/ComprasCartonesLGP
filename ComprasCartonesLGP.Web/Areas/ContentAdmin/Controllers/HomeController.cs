using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ComprasCartonesLGP.Web.Areas.ContentAdmin.Controllers
{
    public class HomeController : Controller
    {
        [Authorize(Roles = "Admin")]
        // GET: ContentAdmin/Home
        public ActionResult Index()
        {
            return View();
        }
    }
}