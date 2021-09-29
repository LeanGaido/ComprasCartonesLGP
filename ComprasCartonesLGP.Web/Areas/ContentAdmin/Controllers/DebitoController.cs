using ComprasCartonesLGP.Dal;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ComprasCartonesLGP.Web.Areas.ContentAdmin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class DebitoController : Controller
    {
        private LGPContext db = new LGPContext();
        // GET: ContentAdmin/Debito
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult AdheridosTarjetaCredito(string searchString, string currentFilter, int? page)
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

            int pageSize = 20;
            int pageNumber = (page ?? 1);
            var adheridos = db.AdhesionCard.Where(x => x.state == "signed").ToList();

            if (!string.IsNullOrEmpty(searchString))
            {
                adheridos = adheridos.Where(x => x.card_holder_name.ToUpper().Contains(searchString.ToUpper())).ToList();
            }

            return View(adheridos.ToPagedList(pageNumber, pageSize));
        }

        public ActionResult AdheridosCbu(string searchString, string currentFilter, int? page)
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

            int pageSize = 20;
            int pageNumber = (page ?? 1);
            var adheridos = db.AdhesionCbu.Where(x => x.state == "signed").ToList();

            if (!string.IsNullOrEmpty(searchString))
            {
                adheridos = adheridos.Where(x => x.cbu_holder_name.ToUpper().Contains(searchString.ToUpper())).ToList();
            }

            return View(adheridos.ToPagedList(pageNumber, pageSize));
        }
    }
}