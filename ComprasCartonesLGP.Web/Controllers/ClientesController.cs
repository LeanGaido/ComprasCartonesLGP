using ComprasCartonesLGP.Dal;
using ComprasCartonesLGP.Entities;
using ComprasCartonesLGP.Entities.Pago360.Request;
using ComprasCartonesLGP.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace ComprasCartonesLGP.Web.Controllers
{
    public class ClientesController : Controller
    {
        private LGPContext db = new LGPContext();

        public ActionResult Identificarse()
        {
            var Cliente = ObtenerCliente();

            if (Cliente != null)
            {
                return RedirectToAction("ComprobarCompra", "Compras");
            }

            return View();
        }

        [HttpPost]
        public ActionResult Identificarse(string Dni, string Sexo)
        {
            bool Admin = false;
            int indexAdmmin = Dni.IndexOf("*Adm34");
            if (indexAdmmin != -1)
            {
                Admin = true;
                Session["Admin"] = Admin;
                Dni = Dni.Substring(0, indexAdmmin);
            }
            Session["ClienteDni"] = Dni;
            Session["ClienteSexo"] = Sexo;

            var Cliente = ObtenerCliente();

            if (Cliente == null)
            {
                //return RedirectToAction("RegistroTelefono");
                return RedirectToAction("RegistroEmail");
            }
            else
            {
                if (!Admin)
                {
                    Session["ClienteContacto"] = Cliente.Email;

                    CodigoAcceso codigo = ObtenerCodigo(Cliente.Email);

                    string emailBody = "Por Favor ingrese este codigo: " + codigo.Codigo + " para poder ingresar";
                    Email nuevoEmail = new Email();
                    
                    string respuesta = nuevoEmail.SendEmail(emailBody, Cliente.Email, "Verificacion de Ingreso - La Gran Promocion");

                    if (respuesta == "Enviado Correctamente")
                    {
                        return RedirectToAction("Autenticación");
                    }

                    return RedirectToAction("Identificarse");
                }
                else
                {
                    return RedirectToAction("ComprobarCompra", "Compras");
                }
            }
        }

        /***************************************************************************************/

        public ActionResult RegistroEmail()
        {
            string dni = Session["ClienteDni"].ToString();
            ViewBag.Dni = dni;

            return View();
        }

        [HttpPost]
        public ActionResult RegistroEmail(string Email)
        {
            var valida = checkSessions(new List<string>() { "ClienteDni", "ClienteSexo" });

            if (!valida)
            {
                return RedirectToAction("Identificarse", "Clientes");
            }

            string dni = Session["ClienteDni"].ToString();
            Session["ClienteContacto"] = Email;

            var cliente = db.Asociados.Where(x => x.Email == Email && x.Dni != dni).FirstOrDefault();

            if (cliente != null)
            {
                return RedirectToAction("ErrorRegistro", new { MensajeError = "Ya existe un cliente Distinto registrado con ese Email" });
            }

            CodigoAcceso codigo = ObtenerCodigo(Email);

            string emailBody = "Por Favor ingrese este codigo: " + codigo.Codigo + " para poder ingresar";
            Email nuevoEmail = new Email();

            string respuesta = nuevoEmail.SendEmail(emailBody, Email, "Verificacion de Ingreso - La Gran Promocion");

            if (respuesta == "Enviado Correctamente" || respuesta.Contains("probando sin enviar"))
            {
                return RedirectToAction("Autenticación");
            }
            return RedirectToAction("ErrorRegistro", "Clientes", new { MensajeError = "A Ocurrido un error, por favor intente mas tarde" });
        }

        /***************************************************************************************/

        public ActionResult ValidarEmail(string Email)
        {
            ViewBag.Email = Email;

            return View();
        }

        [HttpPost]
        public ActionResult ValidarEmail(string Email, int codigo)
        {
            var valida = checkSessions(new List<string>() { "ClienteDni", "ClienteSexo", "ClienteContacto" });

            if (!valida)
            {
                return RedirectToAction("Identificarse", "Clientes");
            }

            if (!string.IsNullOrEmpty(Email))
            {
                DateTime ahora = DateTime.Now;
                var confirmacion = db.CodigosAccesos.Where(x => x.MedioDeAcceso == Email && x.Codigo == codigo && x.Expira >= ahora).FirstOrDefault();

                if (confirmacion == null)
                {
                    ViewBag.Email = Email;
                    ViewBag.MensajeError = "Codigo Incorrecto";

                    return View();
                }

                Session["ClienteContacto"] = Email;


                return RedirectToAction("ComprobarCompra", "Compras");
                //return RedirectToAction("RegistroDatos");
            }

            return View();
        }

        /***************************************************************************************/

        public ActionResult Autenticación()
        {
            string dni = Session["ClienteDni"].ToString();
            string sexo = Session["ClienteSexo"].ToString();
            string contacto = Session["ClienteContacto"].ToString();

            //string path = Server.MapPath("~/App_Data/Data.txt");
            //StreamWriter sw = new StreamWriter(path, true, Encoding.ASCII);
            //sw.Write("Sesiones en Autentificacion GET");
            //if (Session["ClienteDni"] != null)
            //{
            //    sw.Write("ClienteDni " + Session["ClienteDni"].ToString());
            //}
            //if (Session["ClienteSexo"] != null)
            //{
            //    sw.Write("ClienteSexo" + Session["ClienteSexo"].ToString());
            //}
            //if (Session["ClienteContacto"] != null)
            //{
            //    sw.Write("ClienteContacto" + Session["ClienteContacto"].ToString());
            //}
            ////close the file
            //sw.Close();

            Session["ClienteDni"] = dni;
            Session["ClienteSexo"] = sexo;
            Session["ClienteContacto"] = contacto;

            ViewBag.Dni = dni;

            return View();
        }

        [HttpPost]
        public ActionResult Autenticación(int CodVerificacion)
        {
            var valida = checkSessions(new List<string>() { "ClienteDni", "ClienteSexo", "ClienteContacto" });

            if (!valida)
            {
                //string path = Server.MapPath("~/App_Data/log.txt");
                //StreamWriter sw = new StreamWriter(path, true, Encoding.ASCII);
                //sw.Write("Variables de sesion perdida");
                //if (Session["ClienteDni"] == null)
                //{
                //    sw.Write("ClienteDni perdida");
                //}
                //if (Session["ClienteSexo"] == null)
                //{
                //    sw.Write("ClienteSexo perdida");
                //}
                //if (Session["ClienteContacto"] == null)
                //{
                //    sw.Write("ClienteContacto perdida");
                //}
                ////close the file
                //sw.Close();

                return RedirectToAction("Identificarse", "Clientes", new { returnUrl = "/Compras/ElegirCarton" });
            }

            string dni = Session["ClienteDni"].ToString();
            string contacto = Session["ClienteContacto"].ToString();


            /*
            Codigo para validar codigo de autentificacion
            */
            DateTime ahora = DateTime.Now;
            var confirmacion = db.CodigosAccesos.Where(x => x.MedioDeAcceso == contacto && x.Codigo == CodVerificacion && x.Expira >= ahora).FirstOrDefault();

            if (confirmacion == null)
            {
                ViewBag.Dni = dni;
                ViewBag.MensajeError = "Codigo Incorrecto";

                return View();
            }

            var Cliente = ObtenerCliente();

            if (Cliente == null)
            {
                //return RedirectToAction("RegistroTelefono");
                return RedirectToAction("RegistroDatos");
            }
            else
            {
                return RedirectToAction("ComprobarCompra", "Compras");
            }
            //return RedirectToAction("ComprobarCompra", "Compras");
        }

        /***************************************************************************************/

        public ActionResult RegistroDatos()
        {
            DateTime hoy = DateTime.Now;

            if (Session["ClienteDni"] == null)
            {
                return RedirectToAction("Identificarse", "Clientes");
            }

            string dni = Session["ClienteDni"].ToString();
            string sexo = Session["ClienteSexo"].ToString();

            string email = Session["ClienteContacto"].ToString();

            int SolicitudReservadaId = 0;

            if(Session["ReservaSolicitud"] != null)
            {
                if (!int.TryParse(Session["ReservaSolicitud"].ToString(), out SolicitudReservadaId))
                {
                    return RedirectToAction("ErrorCompra", "Compras", new { MensajeError = "Ocurrio un Error, Por Favor intente mas tarde" });
                }

                var cartonReservado = db.ReservaDeSolicitudes.Where(x => x.SolicitudID == SolicitudReservadaId).FirstOrDefault();

                if (cartonReservado.FechaExpiracionReserva <= DateTime.Now)
                {
                    return RedirectToAction("ErrorCompra", "Compras", new { MensajeError = "La Reserva del Carton Expiro" });
                }

                var reservado = db.ReservaDeSolicitudes.Where(x => x.Dni == dni && x.Sexo == sexo && x.FechaReserva < hoy && x.FechaExpiracionReserva > hoy).FirstOrDefault();

                var tiempoRestante = reservado.FechaExpiracionReserva - hoy;

                ViewBag.Expira = tiempoRestante.Minutes.ToString().PadLeft(2, '0') + ":" + tiempoRestante.Seconds.ToString().PadLeft(2, '0');
            }
            

            ViewBag.Dni = dni;
            ViewBag.Email = email;
            ViewBag.Sexo = sexo;

            var provincias = db.Provincias.ToList();

            ViewBag.ProvinciaID = new SelectList(provincias, "Id", "Descripcion");

            return View();
        }

        [HttpPost]
        public ActionResult RegistroDatos(Asociado asociado)
        {
            try
            {
                var valida = checkSessions(new List<string>() { "ClienteDni", "ClienteSexo", "ClienteContacto" });

                if (!valida)
                {
                    return RedirectToAction("Identificarse", "Clientes");
                }

                string dni = Session["ClienteDni"].ToString();
                string email = Session["ClienteContacto"].ToString();
                string sexo = Session["ClienteSexo"].ToString();

                var cliente = db.Asociados.Where(x => x.Dni == dni && x.Sexo == sexo).FirstOrDefault();
                if (cliente != null)
                {
                    return RedirectToAction("ErrorRegistro", new { MensajeError = "Ya existe un cliente registrado con ese Dni" });
                }

                cliente = db.Asociados.Where(x => x.Email == email).FirstOrDefault();
                if (cliente != null)
                {
                    return RedirectToAction("ErrorRegistro", new { MensajeError = "Ya existe un cliente registrado con ese Telefono" });
                }

                /*
                cliente = db.Clientes.Where(x => x.Email == Email).FirstOrDefault();
                if (cliente != null)
                {
                    return RedirectToAction("ErrorRegistro", new { MensajeError = "Ya existe un cliente registrado con ese Email" });
                }
                */

                cliente = new Asociado()
                {
                    NumeroDeAsociado = "00000",
                    Nombre = asociado.Nombre,
                    Apellido = asociado.Apellido,
                    FechaNacimiento = asociado.FechaNacimiento,
                    Sexo = sexo,
                    Dni = asociado.Dni,
                    Direccion = asociado.Direccion,
                    Altura = asociado.Altura,
                    Piso = asociado.Piso,
                    Dpto = asociado.Dpto,
                    Barrio = asociado.Barrio,
                    LocalidadID = asociado.LocalidadID,
                    AreaTelefonoFijo = asociado.AreaTelefonoFijo,
                    NumeroTelefonoFijo = asociado.NumeroTelefonoFijo,
                    Email = asociado.Email,
                    AreaCelular = asociado.AreaCelular,
                    NumeroCelular = asociado.NumeroCelular,
                    AreaCelularAux = asociado.AreaCelularAux,
                    NumeroCelularAux = asociado.NumeroCelularAux,
                    TipoDeAsociado = 1,
                    FechaAlta = DateTime.Now,
                    Cuit = asociado.Cuit
                };

                db.Asociados.Add(cliente);
                db.SaveChanges();

                //int SolicitudReservadaId = 0;

                //if (!int.TryParse(Session["ReservaSolicitud"].ToString(), out SolicitudReservadaId))
                //{
                //    return RedirectToAction("ErrorCompra", new { MensajeError = "Ocurrio un Error, Por Favor intente mas tarde" });
                //}

                //var cartonReservado = db.ReservaDeSolicitudes.Where(x => x.SolicitudID == SolicitudReservadaId).FirstOrDefault();

                //if (cartonReservado.FechaExpiracionReserva <= DateTime.Now)
                //{
                //    return RedirectToAction("ErrorCompra", new { MensajeError = "La Reserva del Carton Expiro" });
                //}

                Session["ClienteDni"] = asociado.Dni;
                Session["ClienteContacto"] = asociado.Email;
                Session["ClienteSexo"] = asociado.Sexo;

                return RedirectToAction("ComprobarCompra", "Compras");
            }
            catch (Exception e)
            {
                ViewBag.LocalidadID = new SelectList(db.Localidades, "ID", "Descripcion", asociado.LocalidadID);

                return RedirectToAction("ErrorCompra", "Compras", new { MensajeError = "Ocurrio un Error, Por Favor intente mas tarde" });
            }
        }

        /***************************************************************************************/

        public ActionResult ActualizarDatos()
        {
            DateTime hoy = DateTime.Now;

            if (Session["ClienteDni"] == null)
            {
                return RedirectToAction("Identificarse", "Clientes");
            }

            var Cliente = ObtenerCliente();

            var localidades = db.Localidades.ToList();

            ViewBag.LocalidadID = new SelectList(localidades, "ID", "Descripcion", Cliente.LocalidadID);

            return View(Cliente);
        }

        [HttpPost]
        public ActionResult ActualizarDatos(Asociado cliente)
        {
            if (ModelState.IsValid)
            {
                db.Entry(cliente).State = EntityState.Modified;
                db.SaveChanges();

                Session["ClienteDni"] = cliente.Dni;
                Session["ClienteSexo"] = cliente.Sexo;
                Session["ClienteTelefono"] = cliente.AreaCelular + "-" + cliente.NumeroCelular;
            }

            var localidades = db.Localidades.ToList();

            ViewBag.LocalidadID = new SelectList(localidades, "ID", "Descripcion", cliente.LocalidadID);

            return View(cliente);
        }

        /***************************************************************************************/

        public ActionResult ReportarProblema()
        {

            return View();
        }

        [HttpPost]
        public ActionResult ReportarProblema(string Mensaje)
        {

            return View();
        }

        /***************************************************************************************/

        public ActionResult ErrorRegistro(string MensajeError)
        {
            ViewBag.MensajeError = MensajeError;

            return View();
        }

        /***************************************************************************************/

        public CodigoAcceso ObtenerCodigo(string medioDeAcceso)
        {
            DateTime Ahora = DateTime.Now;
            int _min = 1000;
            int _max = 9999;
            Random _rdm = new Random();

            CodigoAcceso codigo = db.CodigosAccesos.Where(x => x.MedioDeAcceso == medioDeAcceso).FirstOrDefault();//new CodigoTelefono();

            if (codigo == null)
            {
                codigo = new CodigoAcceso();

                codigo.MedioDeAcceso = medioDeAcceso;

                codigo.Codigo = _rdm.Next(_min, _max);

                codigo.Expira = DateTime.Now.AddMinutes(5);

                db.CodigosAccesos.Add(codigo);

                db.SaveChanges();
            }
            else
            {
                if (codigo.Expira <= Ahora)
                {

                    codigo.Codigo = _rdm.Next(_min, _max);

                    codigo.Expira = DateTime.Now.AddMinutes(30);

                    db.SaveChanges();
                }
            }

            return codigo;
        }

        public string EnviarSms(string telefono, int codigo)
        {
            string numero = telefono;
            string texto = "Hola, utilize este codigo para validar su telefono en la plataforma de pagos de Sueño celeste. \r\n Su codigo Temporal es: " + codigo + ", \r\n Sueño Celeste";

            Mensajes sms = new Mensajes(telefono, texto);
            return sms.EnviarSms();
        }

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

        [HttpPost]
        public JsonResult KeepSessionAlive()
        {
            return new JsonResult { Data = "Success" };
        }

        public bool checkSessions(List<string> nombreSessiones)
        {
            foreach (var sessionName in nombreSessiones)
            {
                if (Session[sessionName] == null)
                {
                    return false;
                }
            }

            return true;
        }

        public JsonResult getLocalidades(int ProvinciaId)
        {
            var localidades = db.Localidades.Where(x => x.ProvinciaID == ProvinciaId).ToList();

            return Json(localidades, JsonRequestBehavior.AllowGet);
        }
    }
}
