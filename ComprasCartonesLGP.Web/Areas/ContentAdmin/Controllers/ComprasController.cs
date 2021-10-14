using ComprasCartonesLGP.Dal;
using ComprasCartonesLGP.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace ComprasCartonesLGP.Web.Areas.ContentAdmin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ComprasController : Controller
    {
        private LGPContext db = new LGPContext();
        // GET: ContentAdmin/Compras
        public ActionResult Index()
        {
            var compras = db.ComprasDeSolicitudes.OrderByDescending(x => x.ID).ToList();
            return View(compras);
        }

        public ActionResult Details(int? id)
        {
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
    }
}