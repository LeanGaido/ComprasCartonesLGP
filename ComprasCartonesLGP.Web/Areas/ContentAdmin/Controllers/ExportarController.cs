using ClosedXML.Excel;
using ComprasCartonesLGP.Dal;
using ComprasCartonesLGP.Entities;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ComprasCartonesLGP.Web.Areas.ContentAdmin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ExportarController : Controller
    {
        private LGPContext db = new LGPContext();
        // GET: ContentAdmin/Exportar
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult ExportarClientes()
        {
            return View();
        }

        public FileContentResult ExportarExcelClientes(DateTime fechaInicio, DateTime fechaFin)
        {
            List<Asociado> Asociados = new List<Asociado>();
            try
            {
                Asociados = db.Asociados.Where(x => x.FechaAlta >= fechaInicio && x.FechaAlta <= fechaFin).ToList();
                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("LGP");

                    worksheet.Cell(1, 1).SetValue<string>(Convert.ToString("Nombre y Apellido"));
                    worksheet.Cell(1, 2).SetValue<string>(Convert.ToString("Fecha Nacimiento"));
                    worksheet.Cell(1, 3).SetValue<string>(Convert.ToString("Sexo"));
                    worksheet.Cell(1, 4).SetValue<string>(Convert.ToString("Cuit"));
                    worksheet.Cell(1, 5).SetValue<string>(Convert.ToString("Dni"));
                    worksheet.Cell(1, 6).SetValue<string>(Convert.ToString("Provincia"));
                    worksheet.Cell(1, 7).SetValue<string>(Convert.ToString("Localidad"));
                    worksheet.Cell(1, 8).SetValue<string>(Convert.ToString("Dirección"));
                    worksheet.Cell(1, 9).SetValue<string>(Convert.ToString("Altura"));
                    worksheet.Cell(1, 10).SetValue<string>(Convert.ToString("Barrio"));
                    worksheet.Cell(1, 11).SetValue<string>(Convert.ToString("Piso"));
                    worksheet.Cell(1, 12).SetValue<string>(Convert.ToString("Dpto"));
                    worksheet.Cell(1, 13).SetValue<string>(Convert.ToString("Email"));
                    worksheet.Cell(1, 14).SetValue<string>(Convert.ToString("Nº Celular"));
                    worksheet.Cell(1, 15).SetValue<string>(Convert.ToString("Fecha de alta"));
                    int fila = 2;//"fila" es la fila del excel en la estaria escribiendo y la inicializo en 1
                    foreach (var asociado in Asociados)
                    {                        
                        //Escribo en la fila y en la columna que corresponde para cada valor
                        worksheet.Cell(fila, 1).SetValue<string>(Convert.ToString(asociado.NombreCompleto));
                        worksheet.Cell(fila, 2).SetValue<string>(Convert.ToString(asociado.FechaNacimiento.ToString("dd-MM-yyyy")));

                        string sexo = "";
                        if(asociado.Sexo == "1")
                        {
                            sexo = "Femenino";
                        }
                        if (asociado.Sexo == "2")
                        {
                            sexo = "Masculino";
                        }

                        worksheet.Cell(fila, 3).SetValue<string>(Convert.ToString(sexo));
                        worksheet.Cell(fila, 4).SetValue<string>(Convert.ToString(asociado.Cuit));
                        worksheet.Cell(fila, 5).SetValue<string>(Convert.ToString(asociado.Dni));
                        worksheet.Cell(fila, 6).SetValue<string>(Convert.ToString(asociado.Localidad.Provincia.Descripcion));
                        worksheet.Cell(fila, 7).SetValue<string>(Convert.ToString(asociado.Localidad.Descripcion));
                        worksheet.Cell(fila, 8).SetValue<string>(Convert.ToString(asociado.Direccion));
                        worksheet.Cell(fila, 9).SetValue<string>(Convert.ToString(asociado.Altura));
                        worksheet.Cell(fila, 10).SetValue<string>(Convert.ToString(asociado.Barrio));
                        worksheet.Cell(fila, 11).SetValue<string>(Convert.ToString(asociado.Piso));
                        worksheet.Cell(fila, 12).SetValue<string>(Convert.ToString(asociado.Dpto));
                        worksheet.Cell(fila, 13).SetValue<string>(Convert.ToString(asociado.Email));
                        worksheet.Cell(fila, 14).SetValue<string>(Convert.ToString(asociado.AreaCelular + "-" + asociado.NumeroCelular));
                        worksheet.Cell(fila, 15).SetValue<string>(Convert.ToString(asociado.FechaAlta));
                        fila++;//Avanzo a la sig fila
                    }
                    string newFile = Path.Combine(Server.MapPath("~/Areas/ContentAdmin/Data/Archivos/Clientes/"),"cliente.xlsx");
                    workbook.SaveAs(newFile);

                    String mimeType = MimeMapping.GetMimeMapping(newFile);
                    byte[] stream = System.IO.File.ReadAllBytes(newFile);

                    return File(stream, mimeType);
                }
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public ActionResult ExportarCompras()
        {
            return View();
        }

        public FileContentResult ExportarExcelCompras(DateTime fechaInicio, DateTime fechaFin)
        {
            List<CompraDeSolicitud> compras = new List<CompraDeSolicitud>();
            try
            {
                compras = db.ComprasDeSolicitudes.Where(x => x.FechaVenta >= fechaInicio && x.FechaVenta <= fechaFin).ToList();
                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("LGP");

                    worksheet.Cell(1, 1).SetValue<string>(Convert.ToString("Fecha Compra"));
                    worksheet.Cell(1, 2).SetValue<string>(Convert.ToString("Nº Solicitud"));
                    worksheet.Cell(1, 3).SetValue<string>(Convert.ToString("Nombre Asociado"));
                    worksheet.Cell(1, 4).SetValue<string>(Convert.ToString("Dni"));
                    worksheet.Cell(1, 5).SetValue<string>(Convert.ToString("Cuil"));
                    worksheet.Cell(1, 6).SetValue<string>(Convert.ToString("Fecha Nacimiento"));
                    worksheet.Cell(1, 7).SetValue<string>(Convert.ToString("Sexo"));
                    worksheet.Cell(1, 8).SetValue<string>(Convert.ToString("Calle"));
                    worksheet.Cell(1, 9).SetValue<string>(Convert.ToString("Altura"));
                    worksheet.Cell(1, 10).SetValue<string>(Convert.ToString("Torre"));
                    worksheet.Cell(1, 11).SetValue<string>(Convert.ToString("Piso"));
                    worksheet.Cell(1, 12).SetValue<string>(Convert.ToString("Dpto"));
                    worksheet.Cell(1, 13).SetValue<string>(Convert.ToString("Provincia"));
                    worksheet.Cell(1, 14).SetValue<string>(Convert.ToString("Localidad"));
                    worksheet.Cell(1, 15).SetValue<string>(Convert.ToString("Area Telefono Fijo"));
                    worksheet.Cell(1, 16).SetValue<string>(Convert.ToString("N° Telefono Fijo"));
                    worksheet.Cell(1, 17).SetValue<string>(Convert.ToString("Area Telefono Celular"));
                    worksheet.Cell(1, 18).SetValue<string>(Convert.ToString("N° Telefono Celular"));
                    worksheet.Cell(1, 19).SetValue<string>(Convert.ToString("Email"));
                    worksheet.Cell(1, 20).SetValue<string>(Convert.ToString("Tipo de pago"));
                    worksheet.Cell(1, 21).SetValue<string>(Convert.ToString("Forma de pago"));
                    worksheet.Cell(1, 22).SetValue<string>(Convert.ToString("Estado Pago"));
                    worksheet.Cell(1, 23).SetValue<string>(Convert.ToString("Cantidad de Cuotas"));
                    worksheet.Cell(1, 24).SetValue<string>(Convert.ToString("Total a pagar"));
                    worksheet.Cell(1, 25).SetValue<string>(Convert.ToString("Codigo Vendedor"));
                    int fila = 2;//"fila" es la fila del excel en la estaria escribiendo y la inicializo en 1
                    foreach (var compra in compras)
                    {
                        var asociado = db.Asociados.Where(x => x.ID == compra.AsociadoID).FirstOrDefault();

                        //Escribo en la fila y en la columna que corresponde para cada valor
                        worksheet.Cell(fila, 1).SetValue<string>(Convert.ToString(compra.FechaVenta.ToString("dd-MM-yyyy")));
                        worksheet.Cell(fila, 2).SetValue<string>(Convert.ToString(compra.NroSolicitud));               
                        worksheet.Cell(fila, 3).SetValue<string>(Convert.ToString(asociado.NombreCompleto));
                        worksheet.Cell(fila, 4).SetValue<string>(Convert.ToString(asociado.Dni));
                        worksheet.Cell(fila, 5).SetValue<string>(Convert.ToString(asociado.Cuit));
                        worksheet.Cell(fila, 6).SetValue<string>(Convert.ToString(asociado.FechaNacimiento.ToString("dd-MM-yyyy")));

                        string sexo = "";
                        if (asociado.Sexo == "1")
                        {
                            sexo = "Femenino";
                        }
                        if (asociado.Sexo == "2")
                        {
                            sexo = "Masculino";
                        }

                        worksheet.Cell(fila, 7).SetValue<string>(Convert.ToString(sexo));
                        worksheet.Cell(fila, 8).SetValue<string>(Convert.ToString(asociado.Direccion));
                        worksheet.Cell(fila, 9).SetValue<string>(Convert.ToString(asociado.Altura));
                        worksheet.Cell(fila, 10).SetValue<string>(Convert.ToString(asociado.Torre));
                        worksheet.Cell(fila, 11).SetValue<string>(Convert.ToString(asociado.Piso));
                        worksheet.Cell(fila, 12).SetValue<string>(Convert.ToString(asociado.Dpto));
                        worksheet.Cell(fila, 13).SetValue<string>(Convert.ToString(asociado.Localidad.Provincia.Descripcion));
                        worksheet.Cell(fila, 14).SetValue<string>(Convert.ToString(asociado.Localidad.Descripcion));
                        worksheet.Cell(fila, 15).SetValue<string>(Convert.ToString(asociado.AreaTelefonoFijo));
                        worksheet.Cell(fila, 16).SetValue<string>(Convert.ToString(asociado.NumeroTelefonoFijo));
                        worksheet.Cell(fila, 17).SetValue<string>(Convert.ToString(asociado.AreaCelular));
                        worksheet.Cell(fila, 18).SetValue<string>(Convert.ToString(asociado.NumeroCelular));
                        worksheet.Cell(fila, 19).SetValue<string>(Convert.ToString(asociado.Email));

                        string tipoPago = "";
                        if(compra.TipoDePagoID == 1)
                        {
                            tipoPago = "En un pago";
                        }
                        else
                        {
                            tipoPago = "Plan de cuotas";
                        }

                        worksheet.Cell(fila, 20).SetValue<string>(Convert.ToString(tipoPago));
                        worksheet.Cell(fila, 21).SetValue<string>(Convert.ToString(compra.TipoDePago.Descripcion));

                        var estadoPago = "";
                        if (compra.PagoRealizdo == true)
                        {
                            estadoPago = "Completo";
                        }
                        else
                        {
                            if (compra.PagoCancelado == true)
                            {
                                estadoPago = "Anulado";
                            }
                            else
                            {
                                estadoPago = "Pendiente";
                            }
                        }

                        worksheet.Cell(fila, 22).SetValue<string>(Convert.ToString(estadoPago));
                        worksheet.Cell(fila, 23).SetValue<string>(Convert.ToString(compra.CantCuotas));
                        worksheet.Cell(fila, 24).SetValue<string>(Convert.ToString(compra.TotalAPagar));
                        worksheet.Cell(fila, 25).SetValue<string>(Convert.ToString(compra.CodigoVendedor));
                        fila++;//Avanzo a la sig fila
                    }
                    string newFile = Path.Combine(Server.MapPath("~/Areas/ContentAdmin/Data/Archivos/Compras/"), "compra.xlsx");
                    workbook.SaveAs(newFile);

                    String mimeType = MimeMapping.GetMimeMapping(newFile);
                    byte[] stream = System.IO.File.ReadAllBytes(newFile);

                    return File(stream, mimeType);
                }
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public ActionResult ExportarAdheridosTarjeta()
        {
            return View();
        }

        public FileContentResult ExportarExcelAdheridosTarjeta(DateTime fechaInicio, DateTime fechaFin)
        {
            List<AdhesionCard> adheridos = new List<AdhesionCard>();
            try
            {
                adheridos = db.AdhesionCard.Where(x => x.created_at >= fechaInicio && x.created_at <= fechaFin).ToList();
                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("LGP");

                    worksheet.Cell(1, 1).SetValue<string>(Convert.ToString("Nº Solicitud"));
                    worksheet.Cell(1, 2).SetValue<string>(Convert.ToString("Nombre del titular del servicio"));
                    worksheet.Cell(1, 3).SetValue<string>(Convert.ToString("Nombre del titular de la Tarjeta"));
                    worksheet.Cell(1, 4).SetValue<string>(Convert.ToString("Tarjeta"));
                    worksheet.Cell(1, 5).SetValue<string>(Convert.ToString("Email"));
                    worksheet.Cell(1, 6).SetValue<string>(Convert.ToString("Fecha Adhesión"));
                    worksheet.Cell(1, 7).SetValue<string>(Convert.ToString("Estado Adhesión"));
                    int fila = 2;//"fila" es la fila del excel en la estaria escribiendo y la inicializo en 1
                    foreach (var adherido in adheridos)
                    {
                        //Escribo en la fila y en la columna que corresponde para cada valor
                        worksheet.Cell(fila, 1).SetValue<string>(Convert.ToString(adherido.external_reference));
                        worksheet.Cell(fila, 2).SetValue<string>(Convert.ToString(adherido.adhesion_holder_name));
                        worksheet.Cell(fila, 3).SetValue<string>(Convert.ToString(adherido.card_holder_name));
                        worksheet.Cell(fila, 4).SetValue<string>(Convert.ToString(adherido.card));
                        worksheet.Cell(fila, 5).SetValue<string>(Convert.ToString(adherido.email));
                        worksheet.Cell(fila, 6).SetValue<string>(Convert.ToString(adherido.created_at.ToString("dd-MM-yyyy")));

                        var estado = "";
                        switch (adherido.state)
                        {
                            case "signed":
                                estado = "Adherida";
                                break;
                            case "canceled":
                                estado = "Cancelada";
                                break;
                            case "pending_to_sign":
                                estado= "Pendiente de adhesión";
                                break;
                        }

                        worksheet.Cell(fila, 7).SetValue<string>(Convert.ToString(estado));
                        fila++;//Avanzo a la sig fila
                    }
                    string newFile = Path.Combine(Server.MapPath("~/Areas/ContentAdmin/Data/Archivos/AdheridosCard/"), "adheridosTarjetaCredito.xlsx");
                    workbook.SaveAs(newFile);

                    String mimeType = MimeMapping.GetMimeMapping(newFile);
                    byte[] stream = System.IO.File.ReadAllBytes(newFile);

                    return File(stream, mimeType);
                }
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public ActionResult ExportarAdheridosCbu()
        {
            return View();
        }

        public FileContentResult ExportarExcelAdheridosCbu(DateTime fechaInicio, DateTime fechaFin)
        {
            List<AdhesionCbu> adheridos = new List<AdhesionCbu>();
            try
            {
                adheridos = db.AdhesionCbu.Where(x => x.created_at >= fechaInicio && x.created_at <= fechaFin).ToList();
                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("LGP");

                    worksheet.Cell(1, 1).SetValue<string>(Convert.ToString("Nº Solicitud"));
                    worksheet.Cell(1, 2).SetValue<string>(Convert.ToString("Nombre del titular del servicio"));
                    worksheet.Cell(1, 3).SetValue<string>(Convert.ToString("Nombre del titular de la Tarjeta"));
                    worksheet.Cell(1, 4).SetValue<string>(Convert.ToString("Banco"));
                    worksheet.Cell(1, 5).SetValue<string>(Convert.ToString("Email"));
                    worksheet.Cell(1, 6).SetValue<string>(Convert.ToString("Fecha Adhesión"));
                    worksheet.Cell(1, 7).SetValue<string>(Convert.ToString("Estado Adhesión"));
                    int fila = 2;//"fila" es la fila del excel en la estaria escribiendo y la inicializo en 1
                    foreach (var adherido in adheridos)
                    {
                        //Escribo en la fila y en la columna que corresponde para cada valor
                        worksheet.Cell(fila, 1).SetValue<string>(Convert.ToString(adherido.external_reference));
                        worksheet.Cell(fila, 2).SetValue<string>(Convert.ToString(adherido.adhesion_holder_name));
                        worksheet.Cell(fila, 3).SetValue<string>(Convert.ToString(adherido.cbu_holder_name));
                        worksheet.Cell(fila, 4).SetValue<string>(Convert.ToString(adherido.bank));
                        worksheet.Cell(fila, 5).SetValue<string>(Convert.ToString(adherido.email));
                        worksheet.Cell(fila, 6).SetValue<string>(Convert.ToString(adherido.created_at.ToString("dd-MM-yyyy")));

                        var estado = "";
                        switch (adherido.state)
                        {
                            case "signed":
                                estado = "Adherida";
                                break;
                            case "canceled":
                                estado = "Cancelada";
                                break;
                            case "pending_to_sign":
                                estado = "Pendiente de adhesión";
                                break;
                        }

                        worksheet.Cell(fila, 7).SetValue<string>(Convert.ToString(estado));
                        fila++;//Avanzo a la sig fila
                    }
                    string newFile = Path.Combine(Server.MapPath("~/Areas/ContentAdmin/Data/Archivos/AdheridosCard/"), "adheridosTarjetaCredito.xlsx");
                    workbook.SaveAs(newFile);

                    String mimeType = MimeMapping.GetMimeMapping(newFile);
                    byte[] stream = System.IO.File.ReadAllBytes(newFile);

                    return File(stream, mimeType);
                }
            }
            catch (Exception e)
            {
                return null;
            }
        }
    }
}