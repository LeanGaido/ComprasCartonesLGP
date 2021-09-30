using ComprasCartonesLGP.Dal;
using ComprasCartonesLGP.Entities;
using ComprasCartonesLGP.Entities.Pago360.Request;
using ComprasCartonesLGP.Entities.Pago360.Response;
using ComprasCartonesLGP.Utilities;
using Newtonsoft.Json;
using PagedList;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
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

        public ActionResult ConfirmacionEnvioSolicitudDebitoCbu()
        {
            var ultimaSolicitud = db.DebitosCBU.OrderByDescending(x => x.id).FirstOrDefault();
            if (ultimaSolicitud != null)
            {
                ViewBag.UltimaSolicitud = ultimaSolicitud.created_at;
            }
            return View();
        }

        public ActionResult ConfirmacionEnvioSolicitudDebitoCard()
        {
            var ultimaSolicitud = db.DebitosCard.OrderByDescending(x => x.id).FirstOrDefault();
            if (ultimaSolicitud != null)
            {
                ViewBag.UltimaSolicitud = ultimaSolicitud.created_at;
            }
            return View();
        }

        //public ActionResult DebitarCbu()
        //{
        //    List<Alert> alerts = new List<Alert>();
        //    string errorMessage;

        //    DateTime dateTime = DateTime.Now;
        //    string date = dateTime.ToString("dd-MM-yyyy");
        //    int days = 3;
        //    string primerVencimiento = ObtenerDiaHabil(date, days);
        //    days = 3;
        //    string segundoVencimiento = ObtenerDiaHabil(primerVencimiento, days);
        //    var adheridosCbu = db.AdhesionCbu.Where(x => x.state == "signed").ToList();

        //    foreach (var adherido in adheridosCbu)
        //    {
        //        var ctaCte = db.Cuentas_Corrientes.Where(x => x.codigoAfiliado.ToString() == adherido.external_reference && x.fechaPago == "").OrderByDescending(x => x.id).FirstOrDefault();
        //        var solicitudDebito = db.DebitosCbu.Where(x => x.cupon == ctaCte.numCupo).FirstOrDefault();

        //        if (solicitudDebito == null)
        //        {
        //            DebitCbuPago360Request debito = new DebitCbuPago360Request();
        //            Metadata metadata = new Metadata();

        //            debito.adhesion_id = adherido.id;
        //            debito.first_due_date = primerVencimiento;
        //            debito.first_total = Convert.ToDecimal(ctaCte.importe);
        //            debito.second_due_date = segundoVencimiento;
        //            debito.second_total = Convert.ToDecimal(ctaCte.importe);
        //            debito.description = "LO MAR. Pago periodo:  " + ctaCte.periodo + " a través del débito automático. Cupon: " + ctaCte.numCupo;
        //            metadata.external_reference = ctaCte.numCupo.ToString();
        //            debito.metadata = metadata;

        //            DebitoCBU debitoCbu = new DebitoCBU();
        //            //Respuesta de la Api
        //            string respuesta = "";

        //            //
        //            string debit360Js = JsonConvert.SerializeObject(debito);

        //            //Local
        //            //Uri uri = new Uri("https://localhost:44382/api/RequestDebitCbu?debitRequest=" + HttpUtility.UrlEncode(debit360Js));

        //            //Server
        //            Uri uri = new Uri("http://localhost:90/api/RequestDebitCbu?debitRequest=" + HttpUtility.UrlEncode(debit360Js));

        //            HttpWebRequest requestFile = (HttpWebRequest)WebRequest.Create(uri);

        //            requestFile.ContentType = "application/html";
        //            requestFile.Headers.Add("authorization", "Bearer YjZlOTg2MWMxMzcxYTAwMDUwNmQzZWJlMWUwY2EyZWZjMzU3M2Y3NGE0ZjRkZWU0ZmRlZjcxOGQ4YmY4Yzc4ZQ");

        //            HttpWebResponse webResp = requestFile.GetResponse() as HttpWebResponse;

        //            if (requestFile.HaveResponse)
        //            {
        //                if (webResp.StatusCode == HttpStatusCode.OK || webResp.StatusCode == HttpStatusCode.Accepted)
        //                {
        //                    try
        //                    {
        //                        StreamReader respReader = new StreamReader(webResp.GetResponseStream(), Encoding.GetEncoding("utf-8" /*"iso-8859-1"*/));

        //                        respuesta = respReader.ReadToEnd();

        //                        //DebitCbuPago360Response debitResponse = new DebitCbuPago360Response();
        //                        CbuDebitResponse debitResponse = new CbuDebitResponse();
        //                        //var jsonObject = JObject.Parse(response.Content);

        //                        debitResponse = JsonConvert.DeserializeObject<CbuDebitResponse>(respuesta);

        //                        if (debitResponse.id != 0)
        //                        {
        //                            debitoCbu.id = debitResponse.id;
        //                            debitoCbu.type = debitResponse.type;
        //                            debitoCbu.state = debitResponse.state;
        //                            debitoCbu.created_at = debitResponse.created_at;
        //                            debitoCbu.first_due_date = debitResponse.first_due_date;
        //                            debitoCbu.first_total = debitResponse.first_total;
        //                            debitoCbu.second_due_date = debito.second_due_date;
        //                            debitoCbu.second_total = debitResponse.first_total;
        //                            debitoCbu.description = debitResponse.description;
        //                            debitoCbu.cupon = int.Parse(debitResponse.metadata.external_reference);
        //                            debitoCbu.AdhesionId = debitResponse.Adhesion.id;

        //                            db.DebitosCbu.Add(debitoCbu);
        //                            db.SaveChanges();
        //                        }
        //                        else
        //                        {
        //                            errorMessage = "Socio: " + adherido.external_reference + ". Cupón: " + ctaCte.numCupo + ".";
        //                            alerts.Add(new Alert("danger", errorMessage, true));
        //                        }

        //                    }
        //                    catch (Exception ex)
        //                    {
        //                        errorMessage = "Socio: " + adherido.external_reference + ". Cupón: " + ctaCte.numCupo + ".";
        //                        alerts.Add(new Alert("danger", errorMessage, true));
        //                    }
        //                }
        //                else
        //                {
        //                    errorMessage = "Socio: " + adherido.external_reference + ". Cupón: " + ctaCte.numCupo + ".";
        //                    alerts.Add(new Alert("danger", errorMessage, true));
        //                }
        //            }
        //            else
        //            {
        //                errorMessage = "Socio: " + adherido.external_reference + ". Cupón: " + ctaCte.numCupo + ".";
        //                alerts.Add(new Alert("danger", errorMessage, true));
        //            }
        //        }
        //    }
        //    return View("EnvioSolicitudDebitoExitoso", alerts);
        //}

        //public ActionResult DebitarCard()
        //{
        //    List<Alert> alerts = new List<Alert>();
        //    string errorMessage;

        //    var adheridosCard = db.AdhesionesCard.Where(x => x.state == "signed").ToList();

        //    foreach (var adherido in adheridosCard)
        //    {
        //        var ctaCte = db.Cuentas_Corrientes.Where(x => x.codigoAfiliado.ToString() == adherido.external_reference && x.fechaPago == "").OrderByDescending(x => x.id).FirstOrDefault();
        //        var solicitudDebito = db.DebitosCard.Where(x => x.cupon == ctaCte.numCupo).FirstOrDefault();
        //        if (solicitudDebito == null)
        //        {
        //            DebitCardPago360Request debito = new DebitCardPago360Request();
        //            Metadata metadata = new Metadata();

        //            debito.card_adhesion_id = adherido.id;
        //            debito.month = int.Parse(ctaCte.periodo.Substring(4, 2));
        //            debito.year = int.Parse(ctaCte.periodo.Substring(0, 4));
        //            debito.amount = Convert.ToDecimal(ctaCte.importe);
        //            debito.description = "LO MAR. Pago periodo:  " + ctaCte.periodo + " a través del débito automático. Cupon: " + ctaCte.numCupo;
        //            metadata.external_reference = ctaCte.numCupo.ToString();
        //            debito.metadata = metadata;

        //            DebitoCard debitoCard = new DebitoCard();
        //            //Respuesta de la Api
        //            string respuesta = "";

        //            //
        //            string debit360Js = JsonConvert.SerializeObject(debito);

        //            //Local
        //            //Uri uri = new Uri("https://localhost:44382/api/RequestDebitCard?debitRequest=" + HttpUtility.UrlEncode(debit360Js));

        //            //Server
        //            Uri uri = new Uri("http://localhost:90/api/RequestDebitCard?debitRequest=" + HttpUtility.UrlEncode(debit360Js));

        //            HttpWebRequest requestFile = (HttpWebRequest)WebRequest.Create(uri);

        //            requestFile.ContentType = "application/html";
        //            requestFile.Headers.Add("authorization", "Bearer ZTY0Zjk0NThlOTZlYzFkMmVmYWNjZWZiYzJiZDk1YjQ5ZjAzMDVhYzZhYTExZTE3NTM1ZGYwYjRiMmI2OTQxYQ");

        //            HttpWebResponse webResp = requestFile.GetResponse() as HttpWebResponse;

        //            if (requestFile.HaveResponse)
        //            {
        //                if (webResp.StatusCode == HttpStatusCode.OK || webResp.StatusCode == HttpStatusCode.Accepted)
        //                {
        //                    try
        //                    {
        //                        StreamReader respReader = new StreamReader(webResp.GetResponseStream(), Encoding.GetEncoding("utf-8" /*"iso-8859-1"*/));

        //                        respuesta = respReader.ReadToEnd();

        //                        DebitCardPago360Response debitResponse = new DebitCardPago360Response();

        //                        //var jsonObject = JObject.Parse(response.Content);

        //                        debitResponse = JsonConvert.DeserializeObject<DebitCardPago360Response>(respuesta);
        //                        if (debitResponse.id != 0)
        //                        {
        //                            debitoCard.id = debitResponse.id;
        //                            debitoCard.type = debitResponse.type;
        //                            debitoCard.state = debitResponse.state;
        //                            debitoCard.created_at = debitResponse.created_at;
        //                            debitoCard.amount = debitResponse.amount;
        //                            debitoCard.month = debitResponse.month;
        //                            debitoCard.year = debitResponse.year;
        //                            debitoCard.description = debitResponse.description;
        //                            debitoCard.cupon = int.Parse(debitResponse.metadata.external_reference);
        //                            debitoCard.AdhesionId = debitResponse.card_adhesion.id;

        //                            db.DebitosCard.Add(debitoCard);
        //                            db.SaveChanges();
        //                        }
        //                        else
        //                        {
        //                            errorMessage = "Socio: " + adherido.external_reference + ". Cupón: " + ctaCte.numCupo + ".";
        //                            alerts.Add(new Alert("danger", errorMessage, true));
        //                        }

        //                    }
        //                    catch (Exception ex)
        //                    {
        //                        errorMessage = "Socio: " + adherido.external_reference + ". Cupón: " + ctaCte.numCupo + ".";
        //                        alerts.Add(new Alert("danger", errorMessage, true));
        //                    }
        //                }
        //                else
        //                {
        //                    errorMessage = "Socio: " + adherido.external_reference + ". Cupón: " + ctaCte.numCupo + ".";
        //                    alerts.Add(new Alert("danger", errorMessage, true));
        //                }
        //            }
        //            else
        //            {
        //                errorMessage = "Socio: " + adherido.external_reference + ". Cupón: " + ctaCte.numCupo + ".";
        //                alerts.Add(new Alert("danger", errorMessage, true));
        //            }
        //        }

        //    }
        //    return View("EnvioSolicitudDebitoExitoso", alerts);
        //}

        public string ObtenerDiaHabil(string date, int days)
        {
            string fecha = null;
            NextBusinessDayRequest nextBusinessDay = new NextBusinessDayRequest();
            //Respuesta de la Api
            string respuesta = "";

            nextBusinessDay.date = date;
            nextBusinessDay.days = days;

            //
            string nextBusinessDay360Js = JsonConvert.SerializeObject(nextBusinessDay);

            //Local
            //Uri uri = new Uri("https://localhost:44382/api/RequestNextBusinessDay?nextBusinessDay=" + HttpUtility.UrlEncode(nextBusinessDay360Js));

            //Server
            Uri uri = new Uri("http://localhost:90/api/RequestNextBusinessDay?nextBusinessDay=" + HttpUtility.UrlEncode(nextBusinessDay360Js));

            HttpWebRequest requestFile = (HttpWebRequest)WebRequest.Create(uri);

            requestFile.ContentType = "application/html";
            requestFile.Headers.Add("authorization", "Bearer YjZlOTg2MWMxMzcxYTAwMDUwNmQzZWJlMWUwY2EyZWZjMzU3M2Y3NGE0ZjRkZWU0ZmRlZjcxOGQ4YmY4Yzc4ZQ");

            HttpWebResponse webResp = requestFile.GetResponse() as HttpWebResponse;

            if (requestFile.HaveResponse)
            {
                if (webResp.StatusCode == HttpStatusCode.OK || webResp.StatusCode == HttpStatusCode.Accepted)
                {
                    StreamReader respReader = new StreamReader(webResp.GetResponseStream(), Encoding.GetEncoding("utf-8" /*"iso-8859-1"*/));

                    respuesta = respReader.ReadToEnd();
                    var dia = respuesta.Substring(11, 2);
                    var mes = respuesta.Substring(8, 2);
                    var anio = respuesta.Substring(3, 4);

                    fecha = dia + "-" + mes + "-" + anio;
                }
            }
            return fecha;
        }
    }
}