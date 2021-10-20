using ComprasCartonesLGP.Dal;
using ComprasCartonesLGP.Entities;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace ComprasCartonesLGP.Web.Areas.ContentAdmin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ClientesController : Controller
    {
        private LGPContext db = new LGPContext();
        // GET: ContentAdmin/Clientes
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

            int pageSize = 20;
            int pageNumber = (page ?? 1);

            var clientes = db.Asociados.OrderByDescending(x => x.ID).ToList();
            if (!string.IsNullOrEmpty(searchString))
            {
                clientes = clientes.Where(x => x.NombreCompleto.ToUpper().Contains(searchString.ToUpper())).ToList();
            }

            ViewBag.TotalAsociados = clientes.Count();

            return View(clientes.ToPagedList(pageNumber, pageSize));
        }

        public ActionResult Details(int? id, int? page, string currentFilter)
        {
            ViewBag.page = page;
            ViewBag.CurrentFilter = currentFilter;
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Asociado asociado = db.Asociados.Find(id);
            if (asociado == null)
            {
                return HttpNotFound();
            }
            if(asociado.Sexo == "1")
            {
                ViewBag.Sexo = "Femenino";
            }
            if (asociado.Sexo == "2")
            {
                ViewBag.Sexo = "Masculino";
            }
            return View(asociado);
        }
    }
}