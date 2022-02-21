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
                    int fila = 2;//"fila" es la fila del excel en la estaria escribiendo y la inicializo en 1
                    foreach (var asociado in Asociados)
                    {                        
                        //Escriobo en la fila y en la columna que corresponde para cada valor
                        worksheet.Cell(fila, 1).SetValue<string>(Convert.ToString(asociado.NombreCompleto));
                        worksheet.Cell(fila, 2).SetValue<string>(Convert.ToString(asociado.FechaNacimiento));

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
    }
}