using ComprasCartonesLGP.Dal;
using ComprasCartonesLGP.Entities;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;

namespace ComprasCartonesLGP.Web.Areas.ContentAdmin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ComprasController : Controller
    {
        private LGPContext db = new LGPContext();
        // GET: ContentAdmin/Compras
        public ActionResult Index(string searchString, string currentFilter, int? page, int Estado = 0, int Anio = 0)
        {
            ViewBag.Anio = new SelectList(db.Promociones.OrderByDescending(x => x.Anio), "Anio", "Anio");
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

            if (Anio == 0)
            {
                Anio = DateTime.Now.Year;
            }

            var compras = (from oCompras in db.ComprasDeSolicitudes
                           join oSolicitud in db.Solicitudes on oCompras.SolicitudID equals oSolicitud.ID
                           join oPromocion in db.Promociones on oSolicitud.PromocionId equals oPromocion.ID
                           where oPromocion.Anio == Anio
                           select oCompras).OrderByDescending(x => x.ID).ToList();

            if (!string.IsNullOrEmpty(searchString))
            {
                compras = compras.Where(x => x.NroSolicitud.ToUpper().Contains(searchString.ToUpper())).ToList();
            }

            switch (Estado)
            {
                case 1:
                    compras = compras.Where(x => x.PagoRealizdo == true).ToList();
                    break;
                case 2:
                    compras = compras.Where(x => x.PagoRealizdo == false && x.PagoCancelado == false).ToList();
                    break;
                case 3:
                    compras = compras.Where(x => x.PagoCancelado == true).ToList();
                    break;
            }

            ViewBag.Estado = Estado;
            ViewBag.TotalCompras = compras.Count();
            return View(compras.ToPagedList(pageNumber, pageSize));
        }

        public ActionResult Details(int? id, int? page, string currentFilter, int Estado = 0, int Anio = 0)
        {
            ViewBag.page = page;
            ViewBag.CurrentFilter = currentFilter;
            ViewBag.Estado = Estado;
            ViewBag.Anio = Anio;
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
            if(compra.PagoCancelado == true)
            {
                ViewBag.EstadoPago = "Cancelado";
            }
            else
            {
                if (compra.PagoRealizdo == true)
                {
                    ViewBag.EstadoPago = "Completo";
                }
                else
                {
                    ViewBag.EstadoPago = "Pendiente";
                }
            }
            var localidad = db.Localidades.Where(x => x.ID == asociado.LocalidadID).FirstOrDefault();

            ViewBag.NombreAsociado = asociado.NombreCompleto;
            ViewBag.Dni = asociado.Dni;
            ViewBag.Cuil = asociado.Cuit;
            ViewBag.FechaNacimiento = asociado.FechaNacimiento.ToString("dd-MM-yyyy");
            if(asociado.Sexo == "1")
            {
                ViewBag.Sexo = "Femenino";
            }
            if (asociado.Sexo == "2")
            {
                ViewBag.Sexo = "Masculino";
            }
            ViewBag.Calle = asociado.Direccion;
            ViewBag.Altura = asociado.Altura;
            ViewBag.Torre = asociado.Torre;
            ViewBag.Piso = asociado.Piso;
            ViewBag.Departamento = asociado.Dpto;
            ViewBag.Provincia = localidad.Provincia.Descripcion;
            ViewBag.Localidad = localidad.Descripcion;
            ViewBag.CaracTelFijo = asociado.AreaTelefonoFijo;
            ViewBag.NroTelFijo = asociado.NumeroTelefonoFijo;
            ViewBag.CaracCelu = asociado.AreaCelular;
            ViewBag.NroCelu = asociado.NumeroCelular;
            ViewBag.Email = asociado.Email;

            return View(compra);
        }

        public ActionResult TablaEstadoPago(int? id, int? page, string currentFilter, int Estado = 0, int Anio = 0)
        {
            var cuotas = db.CuotasCompraDeSolicitudes.Where(x => x.CompraDeSolicitudID == id).ToList();
            ViewBag.IdSolicitud = id;
            ViewBag.page = page;
            ViewBag.CurrentFilter = currentFilter;
            ViewBag.Estado = Estado;
            ViewBag.Anio = Anio;
            if (cuotas == null)
            {
                return HttpNotFound();
            }
            return View(cuotas);
        }
    }
}