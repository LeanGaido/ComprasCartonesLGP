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

        public ActionResult DebitarCbu()
        {
            List<Alert> alerts = new List<Alert>();
            string errorMessage;

            DateTime dateTime = DateTime.Now;
            string date = dateTime.ToString("dd-MM-yyyy");
            int days = 3;
            //string primerVencimiento = ObtenerDiaHabil(date, days);
            string primerVencimiento = 3.ToString();
            days = 3;
            //string segundoVencimiento = ObtenerDiaHabil(primerVencimiento, days);
            string segundoVencimiento = 6.ToString();
            var adheridosCbu = db.AdhesionCbu.Where(x => x.state == "signed").ToList();

            foreach (var adherido in adheridosCbu)
            {
                var solicitud = db.ComprasDeSolicitudes.Where(x => x.NroSolicitud == adherido.external_reference && x.PagoRealizdo == false && x.PagoCancelado == false).FirstOrDefault();
                if(solicitud != null)
                {
                    var cuotaSolicitud = db.CuotasCompraDeSolicitudes.Where(x => x.CompraDeSolicitudID == solicitud.ID && x.CuotaPagada == false).FirstOrDefault();
                    var solicitudDebito = db.DebitosCBU.Where(x => x.CuotaId == cuotaSolicitud.ID).FirstOrDefault();

                    if (solicitudDebito == null)
                    {
                        CbuDebitRequest debito = new CbuDebitRequest();
                        Metadata metadata = new Metadata();

                        debito.adhesion_id = adherido.id;
                        debito.first_due_date = primerVencimiento;
                        //debito.first_total = Convert.ToDecimal(ctaCte.importe);
                        debito.first_total = (decimal)cuotaSolicitud.PrimerPrecioCuota;
                        debito.second_due_date = segundoVencimiento;
                        //debito.second_total = Convert.ToDecimal(ctaCte.importe);
                        debito.second_total = (decimal)cuotaSolicitud.SeguntoPrecioCuota;
                        debito.description = "LGP. Pago cuota del mes:  " + cuotaSolicitud.MesCuota + " a través del débito automático. Monto: $" + cuotaSolicitud.PrimerPrecioCuota;
                        metadata.CuotaId = cuotaSolicitud.ID;
                        debito.metadata = metadata;

                        DebitoCBU debitoCbu = new DebitoCBU();
                        //Respuesta de la Api
                        string respuesta = "";

                        //
                        string debit360Js = JsonConvert.SerializeObject(debito);

                        //Local
                        Uri uri = new Uri("https://localhost:44382/api/RequestDebitCbu?debitRequest=" + HttpUtility.UrlEncode(debit360Js));

                        //Server
                        // uri = new Uri("http://localhost:90/api/RequestDebitCbu?debitRequest=" + HttpUtility.UrlEncode(debit360Js));

                        HttpWebRequest requestFile = (HttpWebRequest)WebRequest.Create(uri);

                        requestFile.ContentType = "application/html";
                        requestFile.Headers.Add("authorization", "Bearer YjZlOTg2MWMxMzcxYTAwMDUwNmQzZWJlMWUwY2EyZWZjMzU3M2Y3NGE0ZjRkZWU0ZmRlZjcxOGQ4YmY4Yzc4ZQ");

                        HttpWebResponse webResp = requestFile.GetResponse() as HttpWebResponse;

                        if (requestFile.HaveResponse)
                        {
                            if (webResp.StatusCode == HttpStatusCode.OK || webResp.StatusCode == HttpStatusCode.Accepted)
                            {
                                try
                                {
                                    StreamReader respReader = new StreamReader(webResp.GetResponseStream(), Encoding.GetEncoding("utf-8" /*"iso-8859-1"*/));

                                    respuesta = respReader.ReadToEnd();

                                    //DebitCbuPago360Response debitResponse = new DebitCbuPago360Response();
                                    CbuDebitResponse debitResponse = new CbuDebitResponse();
                                    //var jsonObject = JObject.Parse(response.Content);

                                    debitResponse = JsonConvert.DeserializeObject<CbuDebitResponse>(respuesta);

                                    if (debitResponse.id != 0)
                                    {
                                        debitoCbu.id = debitResponse.id;
                                        debitoCbu.type = debitResponse.type;
                                        debitoCbu.state = debitResponse.state;
                                        debitoCbu.created_at = debitResponse.created_at;
                                        debitoCbu.first_due_date = debitResponse.first_due_date;
                                        debitoCbu.first_total = debitResponse.first_total;
                                        debitoCbu.second_due_date = debito.second_due_date;
                                        debitoCbu.second_total = debitResponse.first_total;
                                        debitoCbu.description = debitResponse.description;
                                        debitoCbu.CuotaId = debitResponse.metadata.CuotaId;
                                        debitoCbu.adhesionId = debitResponse.AdhesionId;

                                        db.DebitosCBU.Add(debitoCbu);
                                        db.SaveChanges();
                                    }
                                    else
                                    {
                                        var asociado = db.Asociados.Where(x => x.ID == solicitud.AsociadoID).FirstOrDefault();
                                        errorMessage = "Socio: " + asociado.NombreCompleto + ". Cuota del mes: " + cuotaSolicitud.MesCuota + ".";
                                        alerts.Add(new Alert("danger", errorMessage, true));
                                    }

                                }
                                catch (Exception ex)
                                {
                                    var asociado = db.Asociados.Where(x => x.ID == solicitud.AsociadoID).FirstOrDefault();
                                    errorMessage = "Socio: " + asociado.NombreCompleto + ". Cuota del mes: " + cuotaSolicitud.MesCuota + ".";
                                    alerts.Add(new Alert("danger", errorMessage, true));
                                }
                            }
                            else
                            {
                                var asociado = db.Asociados.Where(x => x.ID == solicitud.AsociadoID).FirstOrDefault();
                                errorMessage = "Socio: " + asociado.NombreCompleto + ". Cuota del mes: " + cuotaSolicitud.MesCuota + ".";
                                alerts.Add(new Alert("danger", errorMessage, true));
                            }
                        }
                        else
                        {
                            var asociado = db.Asociados.Where(x => x.ID == solicitud.AsociadoID).FirstOrDefault();
                            errorMessage = "Socio: " + asociado.NombreCompleto + ". Cuota del mes: " + cuotaSolicitud.MesCuota + ".";
                            alerts.Add(new Alert("danger", errorMessage, true));
                        }
                    }
                    else
                    {
                        var asociado = db.Asociados.Where(x => x.ID == solicitud.AsociadoID).FirstOrDefault();
                        errorMessage = "Socio: " + asociado.NombreCompleto + ". Cuota del mes: " + cuotaSolicitud.MesCuota + ".";
                        alerts.Add(new Alert("danger", errorMessage, true));
                    }
                }
            }
            return View("EnvioSolicitudDebitoExitoso", alerts);
        }

        public ActionResult DebitarCard()
        {
            List<Alert> alerts = new List<Alert>();
            string errorMessage;

            var adheridosCard = db.AdhesionCard.Where(x => x.state == "signed").ToList();

            foreach (var adherido in adheridosCard)
            {
                var solicitud = db.ComprasDeSolicitudes.Where(x => x.NroSolicitud == adherido.external_reference && x.PagoRealizdo == false && x.PagoCancelado == false).FirstOrDefault();
                if(solicitud != null)
                {
                    var cuotaSolicitud = db.CuotasCompraDeSolicitudes.Where(x => x.CompraDeSolicitudID == solicitud.ID && x.CuotaPagada == false).FirstOrDefault();
                    var solicitudDebito = db.DebitosCard.Where(x => x.CuotaId == cuotaSolicitud.ID).FirstOrDefault();

                    if (solicitudDebito == null)
                    {
                        CardDebitRequest debito = new CardDebitRequest();
                        Metadata metadata = new Metadata();

                        debito.card_adhesion_id = adherido.id;
                        debito.month = Convert.ToInt32(cuotaSolicitud.MesCuota);
                        debito.year = Convert.ToInt32(cuotaSolicitud.AnioCuota);
                        debito.amount = cuotaSolicitud.PrimerPrecioCuota;
                        debito.description = "LGP. Pago cuota del mes:  " + cuotaSolicitud.MesCuota + " a través del débito automático. Monto: $" + cuotaSolicitud.PrimerPrecioCuota;
                        metadata.CuotaId = cuotaSolicitud.ID;
                        debito.metadata = metadata;

                        DebitoCard debitoCard = new DebitoCard();
                        //Respuesta de la Api
                        string respuesta = "";

                        //
                        string debit360Js = JsonConvert.SerializeObject(debito);

                        //Local
                        //Uri uri = new Uri("https://localhost:44382/api/RequestDebitCard?debitRequest=" + HttpUtility.UrlEncode(debit360Js));

                        //Server
                        Uri uri = new Uri("http://localhost:90/api/RequestDebitCard?debitRequest=" + HttpUtility.UrlEncode(debit360Js));

                        HttpWebRequest requestFile = (HttpWebRequest)WebRequest.Create(uri);

                        requestFile.ContentType = "application/html";
                        requestFile.Headers.Add("authorization", "Bearer YjZlOTg2MWMxMzcxYTAwMDUwNmQzZWJlMWUwY2EyZWZjMzU3M2Y3NGE0ZjRkZWU0ZmRlZjcxOGQ4YmY4Yzc4ZQ");

                        HttpWebResponse webResp = requestFile.GetResponse() as HttpWebResponse;

                        if (requestFile.HaveResponse)
                        {
                            if (webResp.StatusCode == HttpStatusCode.OK || webResp.StatusCode == HttpStatusCode.Accepted)
                            {
                                try
                                {
                                    StreamReader respReader = new StreamReader(webResp.GetResponseStream(), Encoding.GetEncoding("utf-8" /*"iso-8859-1"*/));

                                    respuesta = respReader.ReadToEnd();

                                    CardDebitResponse debitResponse = new CardDebitResponse();

                                    //var jsonObject = JObject.Parse(response.Content);

                                    debitResponse = JsonConvert.DeserializeObject<CardDebitResponse>(respuesta);
                                    if (debitResponse.id != 0)
                                    {
                                        debitoCard.id = debitResponse.id;
                                        debitoCard.type = debitResponse.type;
                                        debitoCard.state = debitResponse.state;
                                        debitoCard.created_at = debitResponse.created_at;
                                        debitoCard.amount = debitResponse.amount;
                                        debitoCard.month = debitResponse.month;
                                        debitoCard.year = debitResponse.year;
                                        debitoCard.description = debitResponse.description;
                                        debitoCard.CuotaId = debitResponse.metadata.CuotaId;
                                        debitoCard.adhesionId = debitResponse.adhesion.id;

                                        db.DebitosCard.Add(debitoCard);
                                        db.SaveChanges();
                                    }
                                    else
                                    {
                                        var asociado = db.Asociados.Where(x => x.ID == solicitud.AsociadoID).FirstOrDefault();
                                        errorMessage = "Socio: " + asociado.NombreCompleto + ". Cuota del mes: " + cuotaSolicitud.MesCuota + ".";
                                        alerts.Add(new Alert("danger", errorMessage, true));
                                    }

                                }
                                catch (Exception ex)
                                {
                                    var asociado = db.Asociados.Where(x => x.ID == solicitud.AsociadoID).FirstOrDefault();
                                    errorMessage = "Socio: " + asociado.NombreCompleto + ". Cuota del mes: " + cuotaSolicitud.MesCuota + ".";
                                    alerts.Add(new Alert("danger", errorMessage, true));
                                }
                            }
                            else
                            {
                                var asociado = db.Asociados.Where(x => x.ID == solicitud.AsociadoID).FirstOrDefault();
                                errorMessage = "Socio: " + asociado.NombreCompleto + ". Cuota del mes: " + cuotaSolicitud.MesCuota + ".";
                                alerts.Add(new Alert("danger", errorMessage, true));
                            }
                        }
                        else
                        {
                            var asociado = db.Asociados.Where(x => x.ID == solicitud.AsociadoID).FirstOrDefault();
                            errorMessage = "Socio: " + asociado.NombreCompleto + ". Cuota del mes: " + cuotaSolicitud.MesCuota + ".";
                            alerts.Add(new Alert("danger", errorMessage, true));
                        }
                    }
                }
            }
            return View("EnvioSolicitudDebitoExitoso", alerts);
        }

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
            Uri uri = new Uri("https://localhost:44382/api/RequestNextBusinessDay?nextBusinessDay=" + HttpUtility.UrlEncode(nextBusinessDay360Js));

            //Server
            //Uri uri = new Uri("http://localhost:90/api/RequestNextBusinessDay?nextBusinessDay=" + HttpUtility.UrlEncode(nextBusinessDay360Js));

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