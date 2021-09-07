using ComprasCartonesLGP.Dal;
using ComprasCartonesLGP.Entities;
using ComprasCartonesLGP.Entities.Pago360.Request;
using ComprasCartonesLGP.Entities.Pago360.Response;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;
using ComprasCartonesLGP.Utilities;

namespace ComprasCartonesLGP.Web.Controllers
{
    public class ComprasController : Controller
    {
        private LGPContext db = new LGPContext();
        /***************************************************************************************/

        // GET: Compras
        public ActionResult ComprobarCompra()
        {
            DateTime hoy = DateTime.Now;

            var Cliente = ObtenerCliente();

            if (Cliente != null)
            {
                var compra = db.ComprasDeSolicitudes.Include(t => t.Solicitud).Where(x => x.AsociadoID == Cliente.ID && 
                                                                                          x.Solicitud.Promocion.Anio == hoy.Year && 
                                                                                          x.PagoCancelado == false).ToList();

                if (compra != null && compra.Count > 0) //Check si el cliente ya compro algun carton de este año
                {
                    //EStado de cuenta para todas las solicitudes del cliente
                }
                else
                {
                    return RedirectToAction("ElegirCarton");
                }
            }
            
            return RedirectToAction("ErrorCompra", new { MensajeError = "Ya hay una Compra registrada con este numero de dni." });
        }

        /***************************************************************************************/

        public ActionResult ElegirCarton(int? SearchType, string SearchString)
        {
            DateTime hoy = DateTime.Now;
            if (Session["ClienteDni"] == null || string.IsNullOrEmpty(Session["ClienteDni"].ToString()))
            {
                return RedirectToAction("Identificarse", "Clientes", new { returnUrl = "/Compras/ElegirCarton" });
            }

            //var FechaLimite = db.FechaLimiteVentaCartones.Where(x => x.Año == hoy.Year).FirstOrDefault();
            //if(FechaLimite.Fecha > hoy)
            //{
            //    return RedirectToAction("Identificarse", "Compras");
            //}

            string dni = Session["ClienteDni"].ToString();
            string telefono = Session["ClienteTelefono"].ToString();
            string sexo = Session["ClienteSexo"].ToString();

            string area = telefono.Substring(0, telefono.IndexOf('-'));
            string numero = telefono.Substring(telefono.IndexOf('-') + 1);

            var reservado = db.ReservaDeSolicitudes.Where(x => x.Dni == dni && x.Sexo == sexo && x.FechaReserva < hoy && x.FechaExpiracionReserva > hoy).FirstOrDefault();

            if (reservado != null)
            {
                Session["ReservaSolicitud"] = reservado.ID;

                var Cliente = ObtenerCliente();

                if (Cliente == null)
                {
                    return RedirectToAction("RegistroDatos", "Clientes");
                }
                else
                {
                    return RedirectToAction("index", "Compras");
                }
            }

            #region NumerosCartones Disponibles
            var listaCartones = ObtenerCartonesDisponibles(SearchType, SearchString);

            ViewBag.NumeroCarton = new SelectList(listaCartones, "ID", "Numero");
            #endregion

            return View();
        }

        [HttpPost]
        public ActionResult ReservarCarton(string CodigoVendedor, int SolicitudId)
        {
            DateTime hoy = DateTime.Now;

            if (Session["ClienteDni"] == null)
            {
                return RedirectToAction("Identificarse", "Clientes", new { returnUrl = "/Compras/ElegirCarton" });
            }

            string dni = Session["ClienteDni"].ToString();
            string telefono = Session["ClienteTelefono"].ToString();
            string sexo = Session["ClienteSexo"].ToString();

            string area = telefono.Substring(0, telefono.IndexOf('-'));
            string numero = telefono.Substring(telefono.IndexOf('-') + 1);

            var reservado = db.ReservaDeSolicitudes.Where(x => x.SolicitudID == SolicitudId && x.FechaReserva < hoy && x.FechaExpiracionReserva > hoy).FirstOrDefault();

            if (reservado != null)
            {
                ViewBag.MensajeError = "El Carton esta Reservado o Comprado";

                return RedirectToAction("ElegirCarton", new { MensajeError = "El Carton esta Reservado o Comprado" });
            }

            ReservaDeSolicitud cartonReservado = db.ReservaDeSolicitudes.Where(x => x.Dni == dni && x.Sexo == sexo).FirstOrDefault();

            if (cartonReservado == null) cartonReservado = new ReservaDeSolicitud();

            cartonReservado.SolicitudID = SolicitudId;
            cartonReservado.Dni = dni;
            cartonReservado.Sexo = sexo;
            cartonReservado.FechaReserva = DateTime.Now;
            cartonReservado.FechaExpiracionReserva = DateTime.Now.AddMinutes(10);

            if (cartonReservado.ID == 0) db.ReservaDeSolicitudes.Add(cartonReservado);

            db.SaveChanges();

            Session["ReservaSolicitud"] = cartonReservado.ID;

            var Cliente = ObtenerCliente();

            if (Cliente == null)
            {
                return RedirectToAction("RegistroDatos", "Clientes");
            }
            else
            {
                return RedirectToAction("index", "Compras");
            }
        }

        [HttpPost]
        public ActionResult CancelarReserva()
        {
            try
            {
                DateTime hoy = DateTime.Now;

                var Cliente = ObtenerCliente();
                if (Cliente == null)
                {
                    Cliente = new Asociado();

                    Cliente.Dni = Session["ClienteDni"].ToString();
                    Cliente.Sexo = Session["ClienteSexo"].ToString();
                }

                var cartonReservado = db.ReservaDeSolicitudes.Where(x => x.Dni == Cliente.Dni && x.Sexo == Cliente.Sexo).FirstOrDefault();

                db.ReservaDeSolicitudes.Remove(cartonReservado);

                db.SaveChanges();

                return RedirectToAction("ErrorCompra", new { MensajeError = "La Reserva del Carton Expiro" });
            }
            catch (Exception e)
            {
                return RedirectToAction("ErrorCompra", new { MensajeError = "La Reserva del Carton Expiro" });
            }
        }

        /***************************************************************************************/

        public ActionResult Index()
        {
            DateTime hoy = DateTime.Now;

            var Cliente = ObtenerCliente();

            if (Cliente == null)
            {
                return RedirectToAction("Identificarse", "Clientes", new { returnUrl = "/Compras/Index" });
            }

            int SolicitudReservadaId = 0;

            if (!int.TryParse(Session["ReservaSolicitud"].ToString(), out SolicitudReservadaId))
            {
                return RedirectToAction("ErrorCompra", new { MensajeError = "Ocurrio un Error, Por Favor intente mas tarde" });
            }

            var cartonReservado = db.ReservaDeSolicitudes.Where(x => x.SolicitudID == SolicitudReservadaId).FirstOrDefault();

            if (cartonReservado == null || cartonReservado.FechaExpiracionReserva <= DateTime.Now)
            {
                return RedirectToAction("ErrorCompra", new { MensajeError = "La Reserva del Carton Expiro" });
            }

            var reservado = db.ReservaDeSolicitudes.Where(x => x.Dni == Cliente.Dni && x.Sexo == Cliente.Sexo && x.FechaReserva < hoy && x.FechaExpiracionReserva > hoy).FirstOrDefault();

            var tiempoRestante = reservado.FechaExpiracionReserva - hoy;

            ViewBag.Expira = tiempoRestante.Minutes.ToString().PadLeft(2, '0') + ":" + tiempoRestante.Seconds.ToString().PadLeft(2, '0');

            //var cuotasDebito = ObtenerCuotasDebitoPosibles(cartonReservado.CartonID);

            //ViewBag.CantCuotasDebito = new SelectList(cuotasDebito, "key", "value");

            ViewBag.TipoDePago = new SelectList(db.TiposDePagos.Where(x => x.Activo), "ID", "Descripcion");

            return View();
        }

        [HttpPost]
        public ActionResult Index(int TipoDePago, int InstitucionId, int CantCuotas = 0)
        {
            int CartonVendidoId = 0, PagoCartonId = 0;
            int[] CuotasPlanDePagoId = new int[CantCuotas];
            Pago pago = new Pago();

            DateTime hoy = DateTime.Now;
            string url = "https://www.sueñocelestepago.com.ar/";

            var Cliente = ObtenerCliente();

            if (Cliente == null)
            {
                return RedirectToAction("Identificarse", "Clientes", new { returnUrl = "/Compras/Index" });
            }

            int SolicitudReservadaId = 0;

            if (!int.TryParse(Session["ReservaSolicitud"].ToString(), out SolicitudReservadaId))
            {
                return RedirectToAction("ErrorCompra", new { MensajeError = "Ocurrio un Error, Por Favor intente mas tarde" });
            }

            var cartonReservado = db.ReservaDeSolicitudes.Where(x => x.ID == SolicitudReservadaId && x.Dni == Cliente.Dni).Include("Carton").FirstOrDefault();

            if (cartonReservado == null)
            {
                return RedirectToAction("ErrorCompra", new { MensajeError = "Ocurrio un Error, Por Favor intente mas tarde" });
            }

            if (cartonReservado.FechaExpiracionReserva <= DateTime.Now)
            {
                return RedirectToAction("ErrorCompra", new { MensajeError = "La Reserva del Carton Expiro" });
            }

            try
            {
                var Carton = db.Solicitudes.Where(x => x.ID == cartonReservado.SolicitudID).FirstOrDefault();

                CompraDeSolicitud cartonVendido = new CompraDeSolicitud();

                cartonVendido.NroSolicitud = Carton.NroSolicitud;
                cartonVendido.AsociadoID = Cliente.ID;
                cartonVendido.NroAsociado = Cliente.NumeroDeAsociado;
                cartonVendido.TipoDePagoID = TipoDePago;
                cartonVendido.FechaVenta = hoy;
                cartonVendido.CantCuotas = CantCuotas;
                cartonVendido.EntidadID = InstitucionId;
                cartonVendido.TotalAPagar = Carton.Precio;

                db.ComprasDeSolicitudes.Add(cartonVendido);
                db.SaveChanges();

                CartonVendidoId = cartonVendido.ID;

                #region Envio de Correo
                try
                {
                    string from = "no-reply@xn--sueocelestepago-0qb.com";
                    string usuario = "no-reply@xn--sueocelestepago-0qb.com";
                    string password = "GVbE3UMeME";

                    string subject = "Sueño Celeste la Gran Promocion";

                    string emailBody = ObtenerBodyEmailCompraPagoContado(Carton.NroSolicitud);

                    Email nuevoEmail = new Email();
                    nuevoEmail.SendEmail(emailBody, Cliente.Email, subject);
                }
                catch (Exception)
                {

                }
                #endregion

                if (cartonVendido.TipoDePagoID == 1)
                {
                    CuotaCompraDeSolicitud PagoUnaCuota = new CuotaCompraDeSolicitud();

                    PagoUnaCuota.CompraDeSolicitudID = CartonVendidoId;
                    PagoUnaCuota.MesCuota = hoy.Month.ToString();
                    PagoUnaCuota.AnioCuota = hoy.Year.ToString();

                    PagoUnaCuota.PrimerVencimiento = DateTime.Parse(ObtenerDiaHabil(hoy.ToString("dd-MM-yyyy"),3));
                    PagoUnaCuota.PrimerPrecioCuota = cartonVendido.TotalAPagar;

                    PagoUnaCuota.SeguntoVencimiento = DateTime.Parse(ObtenerDiaHabil(PagoUnaCuota.PrimerVencimiento.ToString("dd-MM-yyyy"), 3));
                    PagoUnaCuota.SeguntoPrecioCuota = cartonVendido.TotalAPagar;

                    PagoUnaCuota.TipoPagoID = 1;

                    db.CuotasCompraDeSolicitudes.Add(PagoUnaCuota);
                    db.SaveChanges();

                    PagoCartonId = PagoUnaCuota.ID;

                    Pago360Request pago360 = new Pago360Request();

                    var numeroCarton = db.Solicitudes.Where(x => x.ID == cartonReservado.SolicitudID).FirstOrDefault();

                    pago360.description = "Pago Total de la Solicitud Nro°: " + numeroCarton.NroSolicitud + " - La Gran Promocion";
                    pago360.first_due_date = PagoUnaCuota.PrimerVencimiento.ToString("dd-MM-yyyy");
                    pago360.first_total = Carton.Precio;
                    pago360.second_due_date = PagoUnaCuota.SeguntoVencimiento.ToString("dd-MM-yyyy");
                    pago360.second_total = Carton.Precio;
                    pago360.payer_name = Cliente.NombreCompleto;
                    pago360.external_reference = PagoCartonId.ToString();
                    pago360.payer_email = Cliente.Email;
                    pago360.back_url_success = url;// + "/PagoRealizado";
                    pago360.back_url_pending = url;// + "/PagoPendiente";
                    pago360.back_url_rejected = url;// + "/PagoCancelado";
                    try
                    {
                        pago = Pagar(pago360);

                        db.Pagos.Add(pago);

                        PagoUnaCuota.PagoID = pago.ID;

                        var ReservaCarton = db.ReservaDeSolicitudes.Where(x => x.ID == SolicitudReservadaId).FirstOrDefault();

                        db.ReservaDeSolicitudes.Remove(ReservaCarton);

                        db.SaveChanges();

                        return Redirect(pago.checkout_url);
                    }
                    catch (Exception e)
                    {
                        if (PagoCartonId != 0)
                        {
                            db.CuotasCompraDeSolicitudes.Remove(PagoUnaCuota);
                            db.SaveChanges();
                        }

                        if (CartonVendidoId != 0)
                        {
                            db.ComprasDeSolicitudes.Remove(cartonVendido);
                            db.SaveChanges();
                        }
                        return RedirectToAction("ErrorCompra", new { MensajeError = "Ocurrio un Error, Por Favor intente mas tarde" });
                    }
                }
                else if (cartonVendido.TipoDePagoID == 2)//Plan de Pagos Debito CBU
                {
                    return RedirectToAction("AdherirseCbu", new { CantCuotas });
                }
                else if (cartonVendido.TipoDePagoID == 3)//Plan de Pagos Debito Tarjeta
                {
                    return RedirectToAction("AdherirseCard", new { CantCuotas });
                }
            }
            catch (Exception e)
            {
                var CartonVendido = db.ComprasDeSolicitudes.Find(CartonVendidoId);
                db.SaveChanges();
                throw;
            }

            return View();
        }

        /***************************************************************************************/

        public ActionResult AdherirseCbu(int CantCuotas)
        {
            DateTime hoy = DateTime.Now;
            var Cliente = ObtenerCliente();

            if (Cliente == null)
            {
                return RedirectToAction("Identificarse", "Clientes", new { returnUrl = "/Compras/ComprobarCompra" });
            }

            int SolicitudReservadaId = 0;

            if (!int.TryParse(Session["ReservaSolicitud"].ToString(), out SolicitudReservadaId))
            {
                return RedirectToAction("ErrorCompra", new { MensajeError = "Ocurrio un Error, Por Favor intente mas tarde" });
            }

            var cartonReservado = db.ReservaDeSolicitudes.Where(x => x.SolicitudID == SolicitudReservadaId).FirstOrDefault();

            if (cartonReservado.FechaExpiracionReserva <= DateTime.Now)
            {
                return RedirectToAction("ErrorCompra", new { MensajeError = "La Reserva del Carton Expiro" });
            }

            var reservado = db.ReservaDeSolicitudes.Where(x => x.Dni == Cliente.Dni && x.Sexo == Cliente.Sexo && x.FechaReserva < hoy && x.FechaExpiracionReserva > hoy).FirstOrDefault();

            var tiempoRestante = reservado.FechaExpiracionReserva - hoy;

            ViewBag.Expira = tiempoRestante.Minutes.ToString().PadLeft(2, '0') + ":" + tiempoRestante.Seconds.ToString().PadLeft(2, '0');

            ViewBag.CantCuotas = CantCuotas;

            return View(Cliente);
        }

        [HttpPost]
        public ActionResult AdherirseCbu(int CantCuotas, string Email, string adhesion_holder_name, long cbu_holder_id_number, string cbu_number)
        {
            DateTime hoy = DateTime.Now;

            var Cliente = ObtenerCliente();

            if (Cliente == null)
            {
                return RedirectToAction("Identificarse", "Clientes", new { returnUrl = "/Compras/ComprobarCompra" });
            }

            int SolicitudReservadaId = 0;

            if (!int.TryParse(Session["ReservaSolicitud"].ToString(), out SolicitudReservadaId))
            {
                return RedirectToAction("ErrorCompra", new { MensajeError = "Ocurrio un Error, Por Favor intente mas tarde" });
            }

            AdhesionCbuPago360Request adhesionPago360 = new AdhesionCbuPago360Request();

            var CartonComprado = db.ComprasDeSolicitudes.Where(x => x.AsociadoID == Cliente.ID && x.FechaVenta.Year == hoy.Year && x.PagoCancelado == false).FirstOrDefault();

            var ReservaCarton = db.ReservaDeSolicitudes.Where(x => x.SolicitudID == SolicitudReservadaId).FirstOrDefault();

            db.ReservaDeSolicitudes.Remove(ReservaCarton);

            adhesionPago360.adhesion_holder_name = adhesion_holder_name;
            adhesionPago360.email = Email;
            adhesionPago360.description = "Adhesion para el Debito automatico de La Gran Promocion";
            adhesionPago360.short_description = "LGP";
            adhesionPago360.external_reference = Cliente.ID.ToString();
            adhesionPago360.cbu_number = cbu_number;
            adhesionPago360.cbu_holder_id_number = cbu_holder_id_number;
            adhesionPago360.cbu_holder_name = adhesion_holder_name;

            var adhesion = GenerarAdhesion(adhesionPago360);
            db.AdhesionCbu.Add(adhesion);

            db.SaveChanges();

            int c = 1, mesInicio = hoy.Month;
            float precioCuota = CartonComprado.Solicitud.Precio / CantCuotas;

            if (hoy.Day >= 7)
            {
                mesInicio++;
            }
            for (int mes = mesInicio; mes < mesInicio + CantCuotas; mes++)
            {
                CuotaCompraDeSolicitud cuota = new CuotaCompraDeSolicitud();

                cuota.CompraDeSolicitudID = CartonComprado.ID;
                cuota.MesCuota = mes.ToString();
                cuota.AnioCuota = hoy.Year.ToString();
                cuota.PrimerVencimiento = new DateTime(hoy.Year, mes, 15);
                cuota.PrimerPrecioCuota = precioCuota;
                cuota.SeguntoVencimiento = new DateTime(hoy.Year, mes, 20);
                cuota.SeguntoPrecioCuota = precioCuota;

                db.CuotasCompraDeSolicitudes.Add(cuota);
                c++;
            }

            db.SaveChanges();

            return RedirectToAction("DebitoPendiente");
        }

        public ActionResult CancelarAdhesionCbu()
        {
            //if (Session["ClienteDni"] == null)
            //{
            //    return RedirectToAction("Identificarse", "Clientes");
            //}

            return View();
        }

        [HttpPost]
        public ActionResult CancelarAdhesionCbu(int Respuesta)
        {
            DateTime hoy = DateTime.Now;
            string url = "http://www.sueñocelestepagos.com.ar/Compras";

            var Cliente = ObtenerCliente();

            if (Cliente == null)
            {
                return RedirectToAction("Identificarse", "Clientes", new { returnUrl = "/Compras/CancelarAdhesion" });
            }

            var CartonComprado = db.ComprasDeSolicitudes.AsNoTracking().Include(t => t.Solicitud).Where(x => x.AsociadoID == Cliente.ID && x.FechaVenta.Year == hoy.Year && x.PagoCancelado == false).FirstOrDefault();

            var adhesion = db.AdhesionCbu.Where(x => x.external_reference == CartonComprado.ID.ToString()).FirstOrDefault();

            AdhesionCbu adhesion360 = CancelarAdhesionCbu360(adhesion.id);

            if (adhesion360.state == "canceled")
            {
                switch (Respuesta)
                {
                    case 1://1 Cancelar Compra de Carton
                        {
                            CartonComprado.PagoCancelado = true;

                            db.SaveChanges();
                            break;
                        }
                    case 2://2 Pagar restante de un Pago
                        {
                            var totalPagar = db.CuotasCompraDeSolicitudes.Where(x => x.CompraDeSolicitudID == CartonComprado.ID &&
                                                                                     x.CuotaPagada == false)
                                                                         .Select(x => x.PrimerPrecioCuota)
                                                                         .Sum();


                            //var FechaDeVencimiento = db.FechasDeVencimiento.Where(x => x.Mes == hoy.Month && x.Año == hoy.Year).FirstOrDefault();

                            Pago360Request pago360 = new Pago360Request();

                            pago360.description = "Pago Total del saldo restante de la Solicitu Nro°: " + CartonComprado.NroSolicitud + " - La Gran Promocion";
                            pago360.first_due_date = hoy.AddDays(3).ToString("dd-MM-yyyy");
                            pago360.first_total = totalPagar;
                            pago360.second_due_date = hoy.AddDays(5).ToString("dd-MM-yyyy");
                            pago360.second_total = totalPagar;
                            pago360.payer_name = Cliente.NombreCompleto;
                            pago360.external_reference = CartonComprado.ID.ToString();
                            pago360.payer_email = Cliente.Email;
                            pago360.back_url_success = url + "/PagoRealizado";
                            pago360.back_url_pending = url + "/PagoPendiente";
                            pago360.back_url_rejected = url + "/PagoCancelado";
                            pago360.excluded_channels = new string[] { "credit_card" };

                            var pago = Pagar(pago360);
                            CartonComprado.TipoDePagoID = 1;

                            db.Pagos.Add(pago);

                            db.SaveChanges();
                            break;
                        }
                }
            }

            return View();
        }

        /***************************************************************************************/

        public Pago Pagar(Pago360Request pago360)
        {
            //retorno
            Pago pago = new Pago();

            //Respuesta de la Api
            string respuesta = "";

            //
            string pago360Js = JsonConvert.SerializeObject(pago360);

            //Local
            //Uri uri = new Uri("https://localhost:44382/api/Payment360?paymentRequest=" + HttpUtility.UrlEncode(pago360Js));

            //Server
            Uri uri = new Uri("http://localhost:90/api/Payment360?paymentRequest=" + HttpUtility.UrlEncode(pago360Js));

            HttpWebRequest requestFile = (HttpWebRequest)WebRequest.Create(uri);

            requestFile.ContentType = "application/html";
            requestFile.Headers.Add("authorization", "Bearer OTllZDJlZjA3NmNlOWQ4NzYzYzYzNjljMjU3YTNmZGYxNTQ3MGIwZGI2MjIwNjc2MDJkYjNmNmRiNWUyNTcxOA");

            HttpWebResponse webResp = requestFile.GetResponse() as HttpWebResponse;

            if (requestFile.HaveResponse)
            {
                if (webResp.StatusCode == HttpStatusCode.OK || webResp.StatusCode == HttpStatusCode.Accepted)
                {
                    StreamReader respReader = new StreamReader(webResp.GetResponseStream(), Encoding.GetEncoding("utf-8"/*"iso-8859-1"*/));

                    respuesta = respReader.ReadToEnd();

                    Pago360Response payment = new Pago360Response();

                    //var jsonObject = JObject.Parse(response.Content);

                    payment = JsonConvert.DeserializeObject<Pago360Response>(respuesta);

                    pago.ID = payment.id;
                    pago.type = payment.type;
                    pago.state = payment.state;
                    pago.created_at = payment.created_at;
                    pago.external_reference = payment.external_reference;
                    pago.payer_name = payment.payer_name;
                    pago.payer_email = payment.payer_email;
                    pago.description = payment.description;
                    pago.first_due_date = payment.first_due_date;
                    pago.first_total = payment.first_total;
                    pago.second_due_date = payment.second_due_date;
                    pago.second_total = payment.second_total;
                    pago.barcode = payment.barcode;
                    pago.checkout_url = payment.checkout_url;
                    pago.barcode_url = payment.barcode_url;
                    pago.pdf_url = payment.pdf_url;
                    pago.excluded_channels = (payment.excluded_channels != null) ? string.Join(";", payment.excluded_channels) : "";
                }
            }

            ////requestFile.Headers.Add("authorization", "Bearer OTllZDJlZjA3NmNlOWQ4NzYzYzYzNjljMjU3YTNmZGYxNTQ3MGIwZGI2MjIwNjc2MDJkYjNmNmRiNWUyNTcxOA");

            return pago;
        }

        public AdhesionCbu GenerarAdhesion(AdhesionCbuPago360Request adhesionPago360)
        {
            AdhesionCbu adhesion = new AdhesionCbu();

            //Respuesta de la Api
            string respuesta = "";

            //
            string adhesionPago360Js = JsonConvert.SerializeObject(adhesionPago360);

            //Local
            Uri uri = new Uri("https://localhost:44382/api/Adhesion360?adhesionRequest=" + HttpUtility.UrlEncode(adhesionPago360Js));

            //Server
            //Uri uri = new Uri("http://localhost:90/api/Adhesion360?adhesionRequest=" + HttpUtility.UrlEncode(adhesionPago360Js));

            HttpWebRequest requestFile = (HttpWebRequest)WebRequest.Create(uri);

            requestFile.ContentType = "application/html";
            requestFile.Headers.Add("authorization", "Bearer OTllZDJlZjA3NmNlOWQ4NzYzYzYzNjljMjU3YTNmZGYxNTQ3MGIwZGI2MjIwNjc2MDJkYjNmNmRiNWUyNTcxOA");

            HttpWebResponse webResp = requestFile.GetResponse() as HttpWebResponse;

            if (requestFile.HaveResponse)
            {
                if (webResp.StatusCode == HttpStatusCode.OK || webResp.StatusCode == HttpStatusCode.Accepted)
                {
                    StreamReader respReader = new StreamReader(webResp.GetResponseStream(), Encoding.GetEncoding("utf-8"/*"iso-8859-1"*/));

                    respuesta = respReader.ReadToEnd();

                    AdhesionCbuPago360Response adhesionResponse = new AdhesionCbuPago360Response();

                    //var jsonObject = JObject.Parse(response.Content);

                    adhesionResponse = JsonConvert.DeserializeObject<AdhesionCbuPago360Response>(respuesta);

                    adhesion.id = adhesionResponse.id;
                    adhesion.external_reference = adhesionResponse.external_reference;
                    adhesion.adhesion_holder_name = adhesionResponse.adhesion_holder_name;
                    adhesion.email = adhesionResponse.email;
                    adhesion.cbu_holder_name = adhesionResponse.cbu_holder_name;
                    adhesion.cbu_holder_id_number = adhesionResponse.cbu_holder_id_number;
                    adhesion.cbu_number = adhesionResponse.cbu_number;
                    adhesion.bank = adhesionResponse.bank;
                    adhesion.description = adhesionResponse.description;
                    adhesion.short_description = adhesionResponse.short_description;
                    adhesion.state = adhesionResponse.state;
                    adhesion.created_at = adhesionResponse.created_at;
                }
            }

            return adhesion;
        }

        public AdhesionCbu CancelarAdhesionCbu360(int adhesionId)
        {
            AdhesionCbu adhesion = new AdhesionCbu();

            //Respuesta de la Api
            string respuesta = "";

            //Local
            //Uri uri = new Uri("https://localhost:44382/api/Adhesion360?id=" + adhesionId);

            //Server
            Uri uri = new Uri("http://localhost:90/api/Adhesion360?id=" + adhesionId);

            HttpWebRequest requestFile = (HttpWebRequest)WebRequest.Create(uri);

            requestFile.ContentType = "application/html";
            requestFile.Headers.Add("authorization", "Bearer OTllZDJlZjA3NmNlOWQ4NzYzYzYzNjljMjU3YTNmZGYxNTQ3MGIwZGI2MjIwNjc2MDJkYjNmNmRiNWUyNTcxOA");

            HttpWebResponse webResp = requestFile.GetResponse() as HttpWebResponse;

            if (requestFile.HaveResponse)
            {
                if (webResp.StatusCode == HttpStatusCode.OK || webResp.StatusCode == HttpStatusCode.Accepted)
                {
                    StreamReader respReader = new StreamReader(webResp.GetResponseStream(), Encoding.GetEncoding("utf-8"/*"iso-8859-1"*/));

                    respuesta = respReader.ReadToEnd();

                    AdhesionCbuPago360Response adhesionResponse = new AdhesionCbuPago360Response();

                    //var jsonObject = JObject.Parse(response.Content);

                    adhesionResponse = JsonConvert.DeserializeObject<AdhesionCbuPago360Response>(respuesta);

                    adhesion.id = adhesionResponse.id;
                    adhesion.external_reference = adhesionResponse.external_reference;
                    adhesion.adhesion_holder_name = adhesionResponse.adhesion_holder_name;
                    adhesion.email = adhesionResponse.email;
                    adhesion.cbu_holder_name = adhesionResponse.cbu_holder_name;
                    adhesion.cbu_holder_id_number = adhesionResponse.cbu_holder_id_number;
                    adhesion.cbu_number = adhesionResponse.cbu_number;
                    adhesion.bank = adhesionResponse.bank;
                    adhesion.description = adhesionResponse.description;
                    adhesion.short_description = adhesionResponse.short_description;
                    adhesion.state = adhesionResponse.state;
                    adhesion.created_at = adhesionResponse.created_at;
                    adhesion.canceled_at = adhesionResponse.canceled_at;
                }
            }

            return adhesion;
        }

        /***************************************************************************************/

        public Asociado ObtenerCliente()
        {
            if (Session["ClienteDni"] == null || Session["ClienteSexo"] == null)
            {
                return null;
            }

            string dni = Session["ClienteDni"].ToString();

            string sexo = Session["ClienteSexo"].ToString();

            if (string.IsNullOrEmpty(dni) || string.IsNullOrEmpty(sexo))
            {
                return null;
            }

            return db.Asociados.Where(x => x.Dni == dni && x.Sexo == sexo).FirstOrDefault();
        }

        public List<Solicitud> ObtenerCartonesDisponibles(int? SearchType, string SearchString)
        {
            DateTime hoy = DateTime.Now;

            //var FechaLimite = db.FechaLimiteVentaCartones.Where(x => x.Vigente).FirstOrDefault();

            //int año = hoy.Year;

            //var FechaDeVencimiento = db.FechasDeVencimiento.Where(x => x.Mes == hoy.Month && x.Año == hoy.Year).FirstOrDefault();

            //if (hoy.Month == 12 && hoy > FechaDeVencimiento.PrimerVencimiento)
            //{
            //    hoy = new DateTime(hoy.Year, hoy.Month, 1).AddMonths(1);
            //}

            var compra = db.ComprasDeSolicitudes.Include(t => t.Solicitud).Where(x => x.Solicitud.Promocion.Anio == hoy.Year && x.PagoCancelado == false).ToList();

            var Cartones = db.Solicitudes.Where(x => x.Promocion.Anio == hoy.Year).ToList();

            if (!string.IsNullOrEmpty(SearchString))
            {
                switch (SearchType)
                {
                    case 1:
                        {
                            Cartones = Cartones.Where(x => x.NroCarton.Contains(SearchString)).ToList();
                            break;
                        }
                    case 2:
                        {
                            Cartones = Cartones.Where(x => x.NroCarton.EndsWith(SearchString)).ToList();
                            break;
                        }
                    default:
                        break;
                }
            }

            var listaCartones = Cartones.Where(x => !compra.Any(y => y.NroSolicitud == x.NroSolicitud)).ToList();

            var cartonesReservados = db.ReservaDeSolicitudes.Where(x => x.FechaReserva <= hoy && x.FechaExpiracionReserva >= hoy).ToList();

            listaCartones = listaCartones.Where(x => !cartonesReservados.Any(y => y.SolicitudID == x.ID)).ToList();

            return listaCartones;
        }

        public string ObtenerBodyEmailCompraPagoContado(string NroSolicitud)
        {
            string body = "<meta charset='utf-8'><style type='text/css'> @media only screen and (max-width: 480px) { table { display: block !important; width: 100% !important; } td { width: 480px !important; } }</style><body style='font-family: 'Malgun Gothic', Arial, sans-serif; margin: 0; padding: 0; width: 100%; -webkit-text-size-adjust: none; -webkit-font-smoothing: antialiased;'> <table width='100%' bgcolor='#FFFFFF' border='0' cellspacing='0' cellpadding='0' id='background' style='height: 100% !important; margin: 0; padding: 0; width: 100% !important;'> <tr> <td align='center' valign='top'> <table width='600' border='0' bgcolor='#FFFFFF' cellspacing='0' cellpadding='0' id='header_container'> <tr> <td align='center' valign='top'> <table width='100%' border='0' bgcolor='#474544' cellspacing='0' cellpadding='0' id='header'> <tr> <td valign='top' class='header_content'> <h1 style='color: #F4F4F4; font-size: 24px; text-align: center;'> </h1> </td> </tr> </table> <!-- // END #header --> </td> </tr> </table> <!-- // END #header_container --> <table width='600' border='0' bgcolor='#26abff' cellspacing='0' cellpadding='20' id='preheader'> <tr> <td valign='top'> <table width='100%' border='0' cellspacing='0' cellpadding='0'> <tr> <td valign='top' width='600'> <div class='logo' style='text-align: center;'> <a href='javascript:void(0)'> <img src='https://www.xn--sueoceleste-3db.com/assets/images/logo.png'> </a> </div> </td> </tr> </table> </td> </tr> </table> <!-- // END #preheader --> <table width='600' border='0' bgcolor='#FFFFFF' cellspacing='0' cellpadding='0' id='header_container'> <tr> <td align='center' valign='top'> <table width='100%' border='0' bgcolor='#474544' cellspacing='0' cellpadding='0' id='header'> <tr> <td valign='top' class='header_content'> <h1 style='color: #F4F4F4; font-size: 24px; text-align: center;'> </h1> </td> </tr> </table> <!-- // END #header --> </td> </tr> </table> <!-- // END #header_container --> <table width='600' border='0' bgcolor='#f2f2f2' cellspacing='0' cellpadding='20' id='body_container'> <tr> <td align='center' valign='top' class='body_content'> <table width='100%' border='0' cellspacing='0' cellpadding='20'> <tr> <td valign='top'> <h2 style='font-size: 28px; text-align: justify;'>Notificación por Compra de Cartón de Sueño Celeste</h2> <p style='font-size: 16px; line-height: 22px; text-align: justify;'> Este aviso automático es para confirmar que la compra del cartón Número [NumeroCarton] se ha realizado con éxito. <br><br> En breve, le estaremos enviando el cartón impreso a su domicilio. <br><br> Muchas gracias por su compra. <br><br> Atentamente <br><br> <b>Sueño Celeste</b> </p> </td> </tr> </table> </td> </tr> </table> <!-- // END #body_container --> <table width='600' border='0' bgcolor='#26abff' cellspacing='0' cellpadding='20' id='contact_container'> <tr> <td align='center' valign='top'> <table width='100%' border='0' cellspacing='0' cellpadding='20' id='contact'> <tr> <td> <p style='color: #F4F4F4; font-size: 14px; line-height: 22px; text-align: center;'> San Martín 295, Morteros, Córdoba, Argentina <br> sceleste_mutual9@hotmail.com <br> +54 (3562) 404455 </p> </td> </tr> </table> <!-- // END #contact --> </td> </tr> </table> <!-- // END #contact_container --> </td> </tr> </table> <!-- // END #background --></body>";

            body = body.Replace("[NumeroCarton]", NroSolicitud);

            return body;
        }

        public string ObtenerBodyEmailCompraPagoCuotas(string NroSolicitud)
        {
            string body = "<meta charset='utf-8'><style type='text/css'> @media only screen and (max-width: 480px) { table { display: block !important; width: 100% !important; } td { width: 480px !important; } }</style><body style='font-family: 'Malgun Gothic', Arial, sans-serif; margin: 0; padding: 0; width: 100%; -webkit-text-size-adjust: none; -webkit-font-smoothing: antialiased;'> <table width='100%' bgcolor='#FFFFFF' border='0' cellspacing='0' cellpadding='0' id='background' style='height: 100% !important; margin: 0; padding: 0; width: 100% !important;'> <tr> <td align='center' valign='top'> <table width='600' border='0' bgcolor='#FFFFFF' cellspacing='0' cellpadding='0' id='header_container'> <tr> <td align='center' valign='top'> <table width='100%' border='0' bgcolor='#474544' cellspacing='0' cellpadding='0' id='header'> <tr> <td valign='top' class='header_content'> <h1 style='color: #F4F4F4; font-size: 24px; text-align: center;'> </h1> </td> </tr> </table> <!-- // END #header --> </td> </tr> </table> <!-- // END #header_container --> <table width='600' border='0' bgcolor='#26abff' cellspacing='0' cellpadding='20' id='preheader'> <tr> <td valign='top'> <table width='100%' border='0' cellspacing='0' cellpadding='0'> <tr> <td valign='top' width='600'> <div class='logo' style='text-align: center;'> <a href='javascript:void(0)'> <img src='https://www.xn--sueoceleste-3db.com/assets/images/logo.png'> </a> </div> </td> </tr> </table> </td> </tr> </table> <!-- // END #preheader --> <table width='600' border='0' bgcolor='#FFFFFF' cellspacing='0' cellpadding='0' id='header_container'> <tr> <td align='center' valign='top'> <table width='100%' border='0' bgcolor='#474544' cellspacing='0' cellpadding='0' id='header'> <tr> <td valign='top' class='header_content'> <h1 style='color: #F4F4F4; font-size: 24px; text-align: center;'> </h1> </td> </tr> </table> <!-- // END #header --> </td> </tr> </table> <!-- // END #header_container --> <table width='600' border='0' bgcolor='#f2f2f2' cellspacing='0' cellpadding='20' id='body_container'> <tr> <td align='center' valign='top' class='body_content'> <table width='100%' border='0' cellspacing='0' cellpadding='20'> <tr> <td valign='top'> <h2 style='font-size: 24px; text-align: justify;'>Notificación de Pago en Cuotas Finalizado</h2> <p style='font-size: 16px; line-height: 22px; text-align: justify;'> Este aviso automático es para informar que se ha completado el plan de pago en cuotas por la compra del cartón Número [NumeroCarton] de Sueño Celeste. <br><br> En breve, le estaremos enviando el cartón impreso a su domicilio. <br><br> Muchas gracias. <br><br> Atentamente <br><br> <b>Sueño Celeste</b> </p> </td> </tr> </table> </td> </tr> </table> <!-- // END #body_container --> <table width='600' border='0' bgcolor='#26abff' cellspacing='0' cellpadding='20' id='contact_container'> <tr> <td align='center' valign='top'> <table width='100%' border='0' cellspacing='0' cellpadding='20' id='contact'> <tr> <td> <p style='color: #F4F4F4; font-size: 14px; line-height: 22px; text-align: center;'> San Martín 295, Morteros, Córdoba, Argentina <br> sceleste_mutual9@hotmail.com <br> +54 (3562) 404455 </p> </td> </tr> </table> <!-- // END #contact --> </td> </tr> </table> <!-- // END #contact_container --> </td> </tr> </table> <!-- // END #background --></body>";

            body = body.Replace("[NumeroCarton]", NroSolicitud);

            return body;
        }

        public string ObtenerBodyEmailCompraPagoPendiente(string NroSolicitud)
        {
            string body = "<meta charset='utf-8'><style type='text/css'> @media only screen and (max-width: 480px) { table { display: block !important; width: 100% !important; } td { width: 480px !important; } }</style><body style='font-family: 'Malgun Gothic', Arial, sans-serif; margin: 0; padding: 0; width: 100%; -webkit-text-size-adjust: none; -webkit-font-smoothing: antialiased;'> <table width='100%' bgcolor='#FFFFFF' border='0' cellspacing='0' cellpadding='0' id='background' style='height: 100% !important; margin: 0; padding: 0; width: 100% !important;'> <tr> <td align='center' valign='top'> <table width='600' border='0' bgcolor='#FFFFFF' cellspacing='0' cellpadding='0' id='header_container'> <tr> <td align='center' valign='top'> <table width='100%' border='0' bgcolor='#474544' cellspacing='0' cellpadding='0' id='header'> <tr> <td valign='top' class='header_content'> <h1 style='color: #F4F4F4; font-size: 24px; text-align: center;'> </h1> </td> </tr> </table> </td> </tr> </table> <table width='600' border='0' bgcolor='#26abff' cellspacing='0' cellpadding='20' id='preheader'> <tr> <td valign='top'> <table width='100%' border='0' cellspacing='0' cellpadding='0'> <tr> <td valign='top' width='600'> <div class='logo' style='text-align: center;'> <a href='javascript:void(0)'> <img src='https://www.xn--sueoceleste-3db.com/assets/images/logo.png'> </a> </div> </td> </tr> </table> </td> </tr> </table> <table width='600' border='0' bgcolor='#FFFFFF' cellspacing='0' cellpadding='0' id='header_container'> <tr> <td align='center' valign='top'> <table width='100%' border='0' bgcolor='#474544' cellspacing='0' cellpadding='0' id='header'> <tr> <td valign='top' class='header_content'> <h1 style='color: #F4F4F4; font-size: 24px; text-align: center;'> </h1> </td> </tr> </table> </td> </tr> </table> <table width='600' border='0' bgcolor='#f2f2f2' cellspacing='0' cellpadding='20' id='body_container'> <tr> <td align='center' valign='top' class='body_content'> <table width='100%' border='0' cellspacing='0' cellpadding='20'> <tr> <td valign='top'> <h2 style='font-size: 24px; text-align: justify;'>Notificación de Aviso de Deuda de Pago</h2> <p style='font-size: 16px; line-height: 22px; text-align: justify;'> Este aviso automático es para informar que se existe una deuda de pago por la compra del cartón Número [NumeroCarton] de Sueño Celeste. <br><br> Por favor, realice el pago correspondiente en breve, a fin de que se haga efectiva la compra de su cartón. <br><br> Muchas gracias. <br><br> Atentamente <br><br> <b>Sueño Celeste</b> </p> </td> </tr> </table> </td> </tr> </table> <table width='600' border='0' bgcolor='#26abff' cellspacing='0' cellpadding='20' id='contact_container'> <tr> <td align='center' valign='top'> <table width='100%' border='0' cellspacing='0' cellpadding='20' id='contact'> <tr> <td> <p style='color: #F4F4F4; font-size: 14px; line-height: 22px; text-align: center;'> San Martín 295, Morteros, Córdoba, Argentina <br> sceleste_mutual9@hotmail.com <br> +54 (3562) 404455 </p> </td> </tr> </table> </td> </tr> </table> </td> </tr> </table></body>";

            body = body.Replace("[NumeroCarton]", NroSolicitud);

            return body;
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
            requestFile.Headers.Add("authorization", "Bearer ZTY0Zjk0NThlOTZlYzFkMmVmYWNjZWZiYzJiZDk1YjQ5ZjAzMDVhYzZhYTExZTE3NTM1ZGYwYjRiMmI2OTQxYQ");

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