﻿using ComprasCartonesLGP.Dal;
using ComprasCartonesLGP.Entities;
using ComprasCartonesLGP.Entities.Pago360.Request;
using ComprasCartonesLGP.Entities.Pago360.Response;
using ComprasCartonesLGP.Entities.ViewsModels;
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
            var adheridos = db.AdhesionCard.Where(x => x.state == "signed").OrderByDescending(x => x.id).ToList();

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
            var adheridos = db.AdhesionCbu.Where(x => x.state == "signed").OrderByDescending(x => x.id).ToList();

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
            string primerVencimiento = ObtenerDiaHabil(date, days);
            string segundoVencimiento = ObtenerDiaHabil(primerVencimiento, days);
            var adheridosCbu = db.AdhesionCbu.Where(x => x.state == "signed").ToList();
            foreach (var adherido in adheridosCbu)
            {
                //Primero valida que no se haya enviado ya una cuota este mes para este adherido
                var SolicitudEnviada = db.DebitosCBU.Where(x => x.adhesionId == adherido.id && x.created_at.Month == dateTime.Month).FirstOrDefault();
                if(SolicitudEnviada == null)
                {
                    //Busca la solicitud y valida que esta no sea nula
                    var solicitud = db.ComprasDeSolicitudes.Where(x => x.NroSolicitud == adherido.external_reference && x.PagoRealizdo == false && x.PagoCancelado == false).FirstOrDefault();
                    if (solicitud != null)
                    {
                        //Busca la cuota y valida que esta no sea nula
                        var cuotaSolicitud = db.CuotasCompraDeSolicitudes.Where(x => x.CompraDeSolicitudID == solicitud.ID && x.CuotaPagada == false).FirstOrDefault();
                        if (cuotaSolicitud != null)
                        {
                            //valida que ya no se haya enviado una solicitud para esta cuota
                            var solicitudDebito = db.DebitosCBU.Where(x => x.CuotaId == cuotaSolicitud.ID).FirstOrDefault();
                            if (solicitudDebito == null || solicitudDebito.state == "canceled")
                            {
                                CbuDebitRequest debito = new CbuDebitRequest();
                                Metadata metadata = new Metadata();

                                debito.adhesion_id = adherido.id;
                                debito.first_due_date = primerVencimiento;
                                debito.first_total = (decimal)cuotaSolicitud.PrimerPrecioCuota;
                                debito.second_due_date = segundoVencimiento;
                                debito.second_total = (decimal)cuotaSolicitud.SeguntoPrecioCuota;
                                debito.description = "LGP. Pago cuota:  " + cuotaSolicitud.MesCuota + " a través del débito automático. Monto: $" + cuotaSolicitud.PrimerPrecioCuota;
                                metadata.external_reference = cuotaSolicitud.ID;
                                debito.metadata = metadata;

                                DebitoCBU debitoCbu = new DebitoCBU();
                                //Respuesta de la Api
                                string respuesta = "";

                                //
                                string debit360Js = JsonConvert.SerializeObject(debito);

                                //Local
                                //Uri uri = new Uri("https://localhost:44382/api/RequestDebitCbu?debitRequest=" + HttpUtility.UrlEncode(debit360Js));

                                //Server
                                Uri uri = new Uri("http://localhost:90/api/RequestDebitCbu?debitRequest=" + HttpUtility.UrlEncode(debit360Js));

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
                                                debitoCbu.CuotaId = debitResponse.metadata.external_reference;
                                                debitoCbu.adhesionId = debitResponse.adhesion.id;

                                                db.DebitosCBU.Add(debitoCbu);
                                                db.SaveChanges();
                                            }
                                            else
                                            {
                                                var asociado = db.Asociados.Where(x => x.ID == solicitud.AsociadoID).FirstOrDefault();
                                                errorMessage = "Socio: " + asociado.NombreCompleto + ". Cuota del mes: " + cuotaSolicitud.MesCuota + ". Solicitud: " + solicitud.NroSolicitud;
                                                alerts.Add(new Alert("danger", errorMessage, true));
                                            }

                                        }
                                        catch (Exception ex)
                                        {
                                            var asociado = db.Asociados.Where(x => x.ID == solicitud.AsociadoID).FirstOrDefault();
                                            errorMessage = "Socio: " + asociado.NombreCompleto + ". Cuota del mes: " + cuotaSolicitud.MesCuota + ". Solicitud: " + solicitud.NroSolicitud;
                                            alerts.Add(new Alert("danger", errorMessage, true));
                                        }
                                    }
                                    else
                                    {
                                        var asociado = db.Asociados.Where(x => x.ID == solicitud.AsociadoID).FirstOrDefault();
                                        errorMessage = "Socio: " + asociado.NombreCompleto + ". Cuota del mes: " + cuotaSolicitud.MesCuota + ". Solicitud: " + solicitud.NroSolicitud;
                                        alerts.Add(new Alert("danger", errorMessage, true));
                                    }
                                }
                                else
                                {
                                    var asociado = db.Asociados.Where(x => x.ID == solicitud.AsociadoID).FirstOrDefault();
                                    errorMessage = "Socio: " + asociado.NombreCompleto + ". Cuota del mes: " + cuotaSolicitud.MesCuota + ". Solicitud: " + solicitud.NroSolicitud;
                                    alerts.Add(new Alert("danger", errorMessage, true));
                                }
                            }
                            else if(solicitudDebito.state == "pending")
                            {
                                int mesCuota = Convert.ToInt32(cuotaSolicitud.MesCuota) + 1;
                                var cuotaSolicitud2 = db.CuotasCompraDeSolicitudes.Where(x => x.CompraDeSolicitudID == solicitud.ID && x.CuotaPagada == false && x.MesCuota == mesCuota.ToString()).FirstOrDefault();

                                if(cuotaSolicitud2 != null)
                                {
                                    CbuDebitRequest debito = new CbuDebitRequest();
                                    Metadata metadata = new Metadata();

                                    debito.adhesion_id = adherido.id;
                                    debito.first_due_date = primerVencimiento;
                                    debito.first_total = (decimal)cuotaSolicitud2.PrimerPrecioCuota;
                                    debito.second_due_date = segundoVencimiento;
                                    debito.second_total = (decimal)cuotaSolicitud2.SeguntoPrecioCuota;
                                    debito.description = "LGP. Pago cuota:  " + cuotaSolicitud2.MesCuota + " a través del débito automático. Monto: $" + cuotaSolicitud2.PrimerPrecioCuota;
                                    metadata.external_reference = cuotaSolicitud2.ID;
                                    debito.metadata = metadata;

                                    DebitoCBU debitoCbu = new DebitoCBU();
                                    //Respuesta de la Api
                                    string respuesta = "";

                                    //
                                    string debit360Js = JsonConvert.SerializeObject(debito);

                                    //Local
                                    //Uri uri = new Uri("https://localhost:44382/api/RequestDebitCbu?debitRequest=" + HttpUtility.UrlEncode(debit360Js));

                                    //Server
                                    Uri uri = new Uri("http://localhost:90/api/RequestDebitCbu?debitRequest=" + HttpUtility.UrlEncode(debit360Js));

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
                                                    debitoCbu.CuotaId = debitResponse.metadata.external_reference;
                                                    debitoCbu.adhesionId = debitResponse.adhesion.id;

                                                    db.DebitosCBU.Add(debitoCbu);
                                                    db.SaveChanges();
                                                }
                                                else
                                                {
                                                    var asociado = db.Asociados.Where(x => x.ID == solicitud.AsociadoID).FirstOrDefault();
                                                    errorMessage = "Socio: " + asociado.NombreCompleto + ". Cuota del mes: " + cuotaSolicitud2.MesCuota + ". Solicitud: " + solicitud.NroSolicitud;
                                                    alerts.Add(new Alert("danger", errorMessage, true));
                                                }

                                            }
                                            catch (Exception ex)
                                            {
                                                var asociado = db.Asociados.Where(x => x.ID == solicitud.AsociadoID).FirstOrDefault();
                                                errorMessage = "Socio: " + asociado.NombreCompleto + ". Cuota del mes: " + cuotaSolicitud2.MesCuota + ". Solicitud: " + solicitud.NroSolicitud;
                                                alerts.Add(new Alert("danger", errorMessage, true));
                                            }
                                        }
                                        else
                                        {
                                            var asociado = db.Asociados.Where(x => x.ID == solicitud.AsociadoID).FirstOrDefault();
                                            errorMessage = "Socio: " + asociado.NombreCompleto + ". Cuota del mes: " + cuotaSolicitud2.MesCuota + ". Solicitud: " + solicitud.NroSolicitud;
                                            alerts.Add(new Alert("danger", errorMessage, true));
                                        }
                                    }
                                    else
                                    {
                                        var asociado = db.Asociados.Where(x => x.ID == solicitud.AsociadoID).FirstOrDefault();
                                        errorMessage = "Socio: " + asociado.NombreCompleto + ". Cuota del mes: " + cuotaSolicitud2.MesCuota + ". Solicitud: " + solicitud.NroSolicitud;
                                        alerts.Add(new Alert("danger", errorMessage, true));
                                    }
                                }                                
                            }
                            //else if (solicitudDebito.state == "rejected")
                            //{

                            //}
                        }
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
            var mesActual = DateTime.Now.ToString("MM");
            int mesComparacion = Convert.ToInt32(mesActual);

            foreach (var adherido in adheridosCard)
            {
                var SolicitudEnviada = db.DebitosCard.Where(x => x.adhesionId == adherido.id && x.created_at.Value.Month == mesComparacion).FirstOrDefault();
                if (SolicitudEnviada == null)
                {
                    var solicitud = db.ComprasDeSolicitudes.Where(x => x.NroSolicitud == adherido.external_reference && x.PagoRealizdo == false && x.PagoCancelado == false).FirstOrDefault();
                    if (solicitud != null)
                    {
                        var cuotaSolicitud = db.CuotasCompraDeSolicitudes.Where(x => x.CompraDeSolicitudID == solicitud.ID && x.CuotaPagada == false).FirstOrDefault();
                        if (cuotaSolicitud != null)
                        {
                            var solicitudDebito = db.DebitosCard.Where(x => x.CuotaId == cuotaSolicitud.ID).FirstOrDefault();
                            if (solicitudDebito == null)
                            {
                                //Si ya cerraron las tarjetas se envian para el proximo periodo
                                int diaDelMes = DateTime.Now.Day;
                                int periodo = 0;
                                int año = 0;

                                if (diaDelMes >= 18)
                                {
                                    periodo = Convert.ToInt32(mesActual);
                                    if (periodo == 12)
                                    {
                                        periodo = 01;
                                        año = Convert.ToInt32(cuotaSolicitud.AnioCuota) + 1;
                                    }
                                    else
                                    {
                                        periodo = Convert.ToInt32(mesActual) + 1;
                                        año = Convert.ToInt32(cuotaSolicitud.AnioCuota);
                                    }
                                }
                                else
                                {
                                    periodo = Convert.ToInt32(mesActual);
                                    año = Convert.ToInt32(cuotaSolicitud.AnioCuota);
                                }

                                CardDebitRequest debito = new CardDebitRequest();
                                Metadata metadata = new Metadata();

                                debito.card_adhesion_id = adherido.id;
                                debito.month = periodo;
                                debito.year = año;
                                debito.amount = cuotaSolicitud.PrimerPrecioCuota;
                                debito.description = "LGP. Pago cuota:  " + cuotaSolicitud.MesCuota + " a través del débito automático. Monto: $" + cuotaSolicitud.PrimerPrecioCuota;
                                metadata.external_reference = cuotaSolicitud.ID;
                                debito.metadata = metadata;

                                DebitoCard debitoCard = new DebitoCard();
                                //Respuesta de la Api
                                string respuesta = "";

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
                                                debitoCard.CuotaId = debitResponse.metadata.external_reference;
                                                debitoCard.adhesionId = debitResponse.card_adhesion.id;

                                                db.DebitosCard.Add(debitoCard);
                                                db.SaveChanges();
                                            }
                                            else
                                            {
                                                var asociado = db.Asociados.Where(x => x.ID == solicitud.AsociadoID).FirstOrDefault();
                                                errorMessage = "Socio: " + asociado.NombreCompleto + ". Cuota del mes: " + cuotaSolicitud.MesCuota + ". Solicitud: " + solicitud.NroSolicitud;
                                                alerts.Add(new Alert("danger", errorMessage, true));
                                            }

                                        }
                                        catch (Exception ex)
                                        {
                                            var asociado = db.Asociados.Where(x => x.ID == solicitud.AsociadoID).FirstOrDefault();
                                            errorMessage = "Socio: " + asociado.NombreCompleto + ". Cuota del mes: " + cuotaSolicitud.MesCuota + ". Solicitud: " + solicitud.NroSolicitud;
                                            alerts.Add(new Alert("danger", errorMessage, true));
                                        }
                                    }
                                    else
                                    {
                                        var asociado = db.Asociados.Where(x => x.ID == solicitud.AsociadoID).FirstOrDefault();
                                        errorMessage = "Socio: " + asociado.NombreCompleto + ". Cuota del mes: " + cuotaSolicitud.MesCuota + ". Solicitud: " + solicitud.NroSolicitud;
                                        alerts.Add(new Alert("danger", errorMessage, true));
                                    }
                                }
                                else
                                {
                                    var asociado = db.Asociados.Where(x => x.ID == solicitud.AsociadoID).FirstOrDefault();
                                    errorMessage = "Socio: " + asociado.NombreCompleto + ". Cuota del mes: " + cuotaSolicitud.MesCuota + ". Solicitud: " + solicitud.NroSolicitud;
                                    alerts.Add(new Alert("danger", errorMessage, true));
                                }
                            }
                            else if (solicitudDebito.state == "pending")
                            {
                                int mesCuota = Convert.ToInt32(cuotaSolicitud.MesCuota) + 1;
                                var cuotaSolicitud2 = db.CuotasCompraDeSolicitudes.Where(x => x.CompraDeSolicitudID == solicitud.ID && x.CuotaPagada == false && x.MesCuota == mesCuota.ToString()).FirstOrDefault();

                                if (cuotaSolicitud2 != null)
                                {
                                    //Si ya cerraron las tarjetas se envian para el proximo periodo
                                    int diaDelMes = DateTime.Now.Day;
                                    int periodo = 0;
                                    int año = 0;

                                    if (diaDelMes >= 18)
                                    {
                                        //periodo = Convert.ToInt32(cuotaSolicitud.MesCuota);
                                        periodo = Convert.ToInt32(mesActual);
                                        if (periodo == 12)
                                        {
                                            periodo = 01;
                                            año = Convert.ToInt32(cuotaSolicitud2.AnioCuota) + 1;
                                        }
                                        else
                                        {
                                            periodo = Convert.ToInt32(mesActual) + 1;
                                            año = Convert.ToInt32(cuotaSolicitud2.AnioCuota);
                                        }
                                    }
                                    else
                                    {
                                        periodo = Convert.ToInt32(mesActual);
                                        año = Convert.ToInt32(cuotaSolicitud2.AnioCuota);
                                    }

                                    CardDebitRequest debito = new CardDebitRequest();
                                    Metadata metadata = new Metadata();

                                    debito.card_adhesion_id = adherido.id;
                                    debito.month = periodo;
                                    debito.year = año;
                                    debito.amount = cuotaSolicitud2.PrimerPrecioCuota;
                                    debito.description = "LGP. Pago cuota:  " + cuotaSolicitud2.MesCuota + " a través del débito automático. Monto: $" + cuotaSolicitud2.PrimerPrecioCuota;
                                    metadata.external_reference = cuotaSolicitud2.ID;
                                    debito.metadata = metadata;

                                    DebitoCard debitoCard = new DebitoCard();
                                    //Respuesta de la Api
                                    string respuesta = "";

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
                                                    debitoCard.CuotaId = debitResponse.metadata.external_reference;
                                                    debitoCard.adhesionId = debitResponse.card_adhesion.id;

                                                    db.DebitosCard.Add(debitoCard);
                                                    db.SaveChanges();
                                                }
                                                else
                                                {
                                                    var asociado = db.Asociados.Where(x => x.ID == solicitud.AsociadoID).FirstOrDefault();
                                                    errorMessage = "Socio: " + asociado.NombreCompleto + ". Cuota del mes: " + cuotaSolicitud2.MesCuota + ". Solicitud: " + solicitud.NroSolicitud;
                                                    alerts.Add(new Alert("danger", errorMessage, true));
                                                }

                                            }
                                            catch (Exception ex)
                                            {
                                                var asociado = db.Asociados.Where(x => x.ID == solicitud.AsociadoID).FirstOrDefault();
                                                errorMessage = "Socio: " + asociado.NombreCompleto + ". Cuota del mes: " + cuotaSolicitud2.MesCuota + ". Solicitud: " + solicitud.NroSolicitud;
                                                alerts.Add(new Alert("danger", errorMessage, true));
                                            }
                                        }
                                        else
                                        {
                                            var asociado = db.Asociados.Where(x => x.ID == solicitud.AsociadoID).FirstOrDefault();
                                            errorMessage = "Socio: " + asociado.NombreCompleto + ". Cuota del mes: " + cuotaSolicitud2.MesCuota + ". Solicitud: " + solicitud.NroSolicitud;
                                            alerts.Add(new Alert("danger", errorMessage, true));
                                        }
                                    }
                                    else
                                    {
                                        var asociado = db.Asociados.Where(x => x.ID == solicitud.AsociadoID).FirstOrDefault();
                                        errorMessage = "Socio: " + asociado.NombreCompleto + ". Cuota del mes: " + cuotaSolicitud2.MesCuota + ". Solicitud: " + solicitud.NroSolicitud;
                                        alerts.Add(new Alert("danger", errorMessage, true));
                                    }
                                }                                
                            }
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

        public ActionResult EnvioSolicitudDebitoExitoso(List<Alert> alerts)
        {
            return View(alerts);
        }

        public ActionResult Rechazos()
        {
            return View();
        }

        public ActionResult RechazosTarjetaCredito(string searchString, string currentFilter, int? page, int Anio = 0)
        {
            List<RechazoDebitoVm> rechazosVm = new List<RechazoDebitoVm>();
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
          
            var rechazos = db.DebitosCard.Where(x => x.state == "rejected" && x.created_at.Value.Year == Anio).OrderByDescending(x => x.id).ToList();

            foreach(var rechazo in rechazos)
            {
                var adhesion = db.AdhesionCard.Where(x => x.id == rechazo.adhesionId).FirstOrDefault();
                var cuota = db.CuotasCompraDeSolicitudes.Where(x => x.ID == rechazo.CuotaId).FirstOrDefault();

                RechazoDebitoVm rechazoVm = new RechazoDebitoVm();

                rechazoVm.Id = rechazo.id;
                rechazoVm.NroSolicitud = adhesion.external_reference;
                rechazoVm.NombreAsociado = adhesion.adhesion_holder_name;
                rechazoVm.MesCuota = cuota.MesCuota;
                rechazoVm.FechaRechazo = rechazo.created_at;

                rechazosVm.Add(rechazoVm);
            }

            if (!string.IsNullOrEmpty(searchString))
            {
                rechazosVm = rechazosVm.Where(x => x.NombreAsociado.ToUpper().Contains(searchString.ToUpper())).ToList();
            }

            return View(rechazosVm.ToPagedList(pageNumber, pageSize));
        }

        public ActionResult RechazosCbu(string searchString, string currentFilter, int? page, int Anio = 0)
        {
            List<RechazoDebitoVm> rechazosVm = new List<RechazoDebitoVm>();
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
            var rechazos = db.DebitosCBU.Where(x => x.state == "rejected" && x.created_at.Year == Anio).OrderByDescending(x => x.id).ToList();

            foreach (var rechazo in rechazos)
            {
                var adhesion = db.AdhesionCbu.Where(x => x.id == rechazo.adhesionId).FirstOrDefault();
                var cuota = db.CuotasCompraDeSolicitudes.Where(x => x.ID == rechazo.CuotaId).FirstOrDefault();

                RechazoDebitoVm rechazoVm = new RechazoDebitoVm();

                rechazoVm.Id = rechazo.id;
                rechazoVm.NroSolicitud = adhesion.external_reference;
                rechazoVm.NombreAsociado = adhesion.adhesion_holder_name;
                rechazoVm.MesCuota = cuota.MesCuota;
                rechazoVm.FechaRechazo = rechazo.created_at;

                rechazosVm.Add(rechazoVm);
            }

            if (!string.IsNullOrEmpty(searchString))
            {
                rechazosVm = rechazosVm.Where(x => x.NombreAsociado.ToUpper().Contains(searchString.ToUpper())).ToList();
            }

            return View(rechazosVm.ToPagedList(pageNumber, pageSize));
        }

        public ActionResult DetalleRechazoTarjetaCredito(int? id, string NombreAsociado, string MesCuota, string currentFilter, int? page, int Anio = 0)
        {
            ViewBag.DisplayMensaje = "none";
            ViewBag.DisplayBoton = "none";
            RechazoDebitoVm rechazoVm = new RechazoDebitoVm();
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var rechazo = db.DebitosCard.Where(x => x.id == id).FirstOrDefault();
            var adhesion = db.AdhesionCard.Where(x => x.id == rechazo.adhesionId).FirstOrDefault();

            rechazoVm.Id = rechazo.id;
            rechazoVm.NombreAsociado = NombreAsociado;
            rechazoVm.NroSolicitud = adhesion.external_reference;
            rechazoVm.MesCuota = MesCuota;
            rechazoVm.FechaRechazo = rechazo.created_at;

            if (adhesion.state == "signed")
            {
                var cuota = db.CuotasCompraDeSolicitudes.Where(x => x.ID == rechazo.CuotaId).FirstOrDefault();
                if(cuota.CuotaPagada == false)
                {
                    var solicitudPendiente = db.DebitosCard.Where(x => x.CuotaId == cuota.ID && x.state == "pending").FirstOrDefault();
                    if(solicitudPendiente == null)
                    {
                        ViewBag.DisplayBoton = "";
                        //PUEDE ENVIAR LA SOLICITUD NUEVAMENTE 
                    }
                    else
                    {
                        ViewBag.Type = "warning";
                        ViewBag.Message = "Esta cuota ya tiene otra solicitud de debito pendiente";
                        ViewBag.DisplayMensaje = "";
                        //MENSAJE SOLICITUD EN ESTADO PENDIENTE
                    }
                }
                else
                {
                    ViewBag.Type = "success";
                    ViewBag.Message = "La cuota ya ha sido abonada";
                    ViewBag.DisplayMensaje = "";
                    //MENSAJE LA CUOTA YA HA SIDO ABONADA
                }
            }
            
            return View(rechazoVm);
        }

        public ActionResult DetalleRechazoCbu(int? id, string NombreAsociado, string MesCuota, string currentFilter, int? page, int Anio = 0)
        {
            ViewBag.DisplayMensaje = "none";
            ViewBag.DisplayBoton = "none";
            RechazoDebitoVm rechazoVm = new RechazoDebitoVm();
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var rechazo = db.DebitosCBU.Where(x => x.id == id).FirstOrDefault();
            var adhesion = db.AdhesionCbu.Where(x => x.id == rechazo.adhesionId).FirstOrDefault();
            var cuota = db.CuotasCompraDeSolicitudes.Where(x => x.ID == rechazo.CuotaId).FirstOrDefault();

            rechazoVm.Id = rechazo.id;
            rechazoVm.NombreAsociado = NombreAsociado;
            rechazoVm.NroSolicitud = adhesion.external_reference;
            rechazoVm.MesCuota = MesCuota;
            rechazoVm.FechaRechazo = rechazo.created_at;

            if (adhesion.state == "signed")
            {
                if (cuota.CuotaPagada == false)
                {
                    var solicitudPendiente = db.DebitosCBU.Where(x => x.CuotaId == cuota.ID && x.state == "pending").FirstOrDefault();
                    if (solicitudPendiente == null)
                    {
                        ViewBag.DisplayBoton = "";
                        //PUEDE ENVIAR LA SOLICITUD NUEVAMENTE 
                    }
                    else
                    {
                        ViewBag.Type = "warning";
                        ViewBag.Message = "ESTA CUOTA YA TIENE OTRA SOLICITUD DE DEBITO PENDIENTE";
                        ViewBag.DisplayMensaje = "";
                        //MENSAJE SOLICITUD EN ESTADO PENDIENTE
                    }
                }
                else
                {
                    ViewBag.Type = "success";
                    ViewBag.Message = "LA CUOTA YA HA SIDO ABONADA";
                    ViewBag.DisplayMensaje = "";
                    //MENSAJE LA CUOTA YA HA SIDO ABONADA
                }
            }
            else
            {
                if (cuota.CuotaPagada == false)
                {
                    ViewBag.Type = "danger";
                    ViewBag.Message = "LA ADHESIÓN REFERIDA A ESTA SOLICITUD YA SE HA DADO DE BAJA. LA CUOTA SE ENCUENTRA IMPAGA";
                    ViewBag.DisplayMensaje = "";
                }
                else
                {
                    ViewBag.Type = "success";
                    ViewBag.Message = "LA ADHESIÓN REFERIDA A ESTA SOLICITUD YA SE HA DADO DE BAJA. LA CUOTA SE ENCUENTRA PAGA";
                    ViewBag.DisplayMensaje = "";
                }
            }
            return View(rechazoVm);
        }

        public ActionResult ConfirmacionEnvioSolicitudRechazoCbu(int? id)
        {
            ViewBag.id = id;
            return View();
        }

        public ActionResult DebitarRechazoCbu(int? id)
        {
            string errorMessage;

            DateTime dateTime = DateTime.Now;
            string date = dateTime.ToString("dd-MM-yyyy");
            int days = 3;
            string primerVencimiento = ObtenerDiaHabil(date, days);
            string segundoVencimiento = ObtenerDiaHabil(primerVencimiento, days);

            var debitoRechazado = db.DebitosCBU.Where(x => x.id == id).FirstOrDefault();
            if (debitoRechazado == null)
            {
                return HttpNotFound();
            }

            var adherido = db.AdhesionCbu.Where(x => x.id == debitoRechazado.adhesionId && x.state == "signed").FirstOrDefault();
            if (adherido == null)
            {
                return HttpNotFound();
            }

            var cuota = db.CuotasCompraDeSolicitudes.Where(x => x.ID == debitoRechazado.CuotaId && x.CuotaPagada == false).FirstOrDefault();
            if (cuota == null)
            {
                return HttpNotFound();
            }

            CbuDebitRequest debito = new CbuDebitRequest();
            Metadata metadata = new Metadata();

            debito.adhesion_id = adherido.id;
            debito.first_due_date = primerVencimiento;
            debito.first_total = (decimal)cuota.PrimerPrecioCuota;
            debito.second_due_date = segundoVencimiento;
            debito.second_total = (decimal)cuota.SeguntoPrecioCuota;
            debito.description = "LGP. Pago cuota:  " + cuota.MesCuota + " a través del débito automático. Monto: $" + cuota.PrimerPrecioCuota;
            metadata.external_reference = cuota.ID;
            debito.metadata = metadata;

            DebitoCBU debitoCbu = new DebitoCBU();
            //Respuesta de la Api
            string respuesta = "";

            //
            string debit360Js = JsonConvert.SerializeObject(debito);

            //Local
            //Uri uri = new Uri("https://localhost:44382/api/RequestDebitCbu?debitRequest=" + HttpUtility.UrlEncode(debit360Js));

            //Server
            Uri uri = new Uri("http://localhost:90/api/RequestDebitCbu?debitRequest=" + HttpUtility.UrlEncode(debit360Js));

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
                            debitoCbu.CuotaId = debitResponse.metadata.external_reference;
                            debitoCbu.adhesionId = debitResponse.adhesion.id;

                            db.DebitosCBU.Add(debitoCbu);
                            db.SaveChanges();
                        }
                        else
                        {
                            return RedirectToAction("ErrorEnvioSolicitudDebito");
                        }

                    }
                    catch (Exception ex)
                    {
                        return RedirectToAction("ErrorEnvioSolicitudDebito");
                    }
                }
                else
                {
                    return RedirectToAction("ErrorEnvioSolicitudDebito");
                }
            }

            return RedirectToAction("EnvioSolicitudDebitoRechazoExitoso");
        }

        public ActionResult ErrorEnvioSolicitudDebito()
        {
            return View();
        }

        public ActionResult EnvioSolicitudDebitoRechazoExitoso()
        {
            return View();
        }
    }
}