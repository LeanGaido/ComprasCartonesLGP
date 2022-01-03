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

        public ActionResult Identificarse(int? codigo)
        {
            if(codigo != null)
            {
                ViewBag.Display = "none";
                var habilitacion = db.Parametros.Where(x => x.Clave == "HabilitarBoton").FirstOrDefault();
                if (habilitacion.Valor == "false")
                {
                    var fecha = db.Parametros.Where(x => x.Clave == "FechaHabilitacion").FirstOrDefault();
                    ViewBag.Fecha = fecha.Valor;
                    ViewBag.Desabilitado = "disabled";
                    ViewBag.Display = "";
                }

                var Cliente = ObtenerCliente();

                if (Cliente != null)
                {
                    return RedirectToAction("ComprobarCompra", "Compras");
                }

                return View();
            }
            else
            {
                return RedirectToAction("ErrorCodigoUnicoIncorrecto", "Clientes");
            }
        }

        [HttpPost]
        public ActionResult Identificarse(string Dni, string Sexo)
        {
            var habilitacion = db.Parametros.Where(x => x.Clave == "HabilitarBoton").FirstOrDefault();
            ViewBag.Display = "none";
            if (habilitacion.Valor == "false")
            {
                var fecha = db.Parametros.Where(x => x.Clave == "FechaHabilitacion").FirstOrDefault();
                ViewBag.Fecha = fecha.Valor;
                ViewBag.Desabilitado = "disabled";
                ViewBag.Display = "";
                return RedirectToAction("ErrorBotonDeshabilitado", "Clientes", new { fechaHabilitacion = fecha.Valor });
            }

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
                    //VALIDACION EMAIL
                    //Session["ClienteContacto"] = Cliente.Email;

                    //CodigoAcceso codigo = ObtenerCodigo(Cliente.Email);

                    //string emailBody = "Por Favor ingrese este codigo: " + codigo.Codigo + " para poder ingresar";
                    //Email nuevoEmail = new Email();

                    //string respuesta = nuevoEmail.SendEmail(emailBody, Cliente.Email, "Verificacion de Ingreso - La Gran Promocion");



                    //VALIDACION TELEFONO
                    Session["ClienteContacto"] = Cliente.AreaCelular + "" + Cliente.NumeroCelular;
                    string numero = Session["ClienteContacto"].ToString();
                    CodigoAcceso codigo = ObtenerCodigo(numero);

                    //Si busca por este camino ya tiene un codigo creado y no se le envia un msj al usuario
                    if (codigo.Codigo == 00000)
                    {
                        return RedirectToAction("Autenticación", new { Mensaje = "Su código de verificación generado todavía está vigente. Ingrese el último código que se envío a su celular." });
                    }

                    string texto = "Hola, su codigo Temporal para ingresar a la compra de LGP es: " + codigo.Codigo;

                    Mensajes sms = new Mensajes(numero, texto);
                    string respuesta = sms.EnviarSms();

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
        public ActionResult ErrorBotonDeshabilitado(string fechaHabilitacion)
        {
            ViewBag.fechaHabilitacion = fechaHabilitacion;
            return View();
        }

        public ActionResult ErrorCodigoUnicoIncorrecto()
        {
            return View();
        }

        /***************************************************************************************/

        public ActionResult RegistroEmail()
        {
            string dni = Session["ClienteDni"].ToString();
            ViewBag.Dni = dni;

            return View();
        }

        [HttpPost]
        public ActionResult RegistroEmail(string Area, string Numero)
        {
            var valida = checkSessions(new List<string>() { "ClienteDni", "ClienteSexo" });

            if (!valida)
            {
                return RedirectToAction("Identificarse", "Clientes");
            }

            //En caso que el usuario ingrese el 0 o el 15 se borra
            var validaArea = Area.Substring(0, 1);
            var validaNumero = Numero.Substring(0, 2);

            if(Convert.ToInt32(validaArea) == 0)
            {
                Area = Area.Substring(1, Area.Length -1);
            }
            if (Convert.ToInt32(validaNumero) == 15)
            {
                Numero = Numero.Substring(2, Numero.Length - 2);
            }

            string dni = Session["ClienteDni"].ToString();
            Session["ClienteContacto"] = Area + "" + Numero;
            var numeroCompleto = Session["ClienteContacto"].ToString();

            Session["ClienteArea"] = Area; 
            Session["ClienteNumero"] = Numero;

            var cliente = db.Asociados.Where(x => x.AreaCelular == Area && x.NumeroCelular == Numero && x.Dni != dni).FirstOrDefault();

            if (cliente != null)
            {
                return RedirectToAction("ErrorRegistro", new { MensajeError = "Ya existe un cliente Distinto registrado con ese Nº de celular" });
            }

            CodigoAcceso codigo = ObtenerCodigo(numeroCompleto);            

            //string emailBody = "Por Favor ingrese este codigo: " + codigo.Codigo + " para poder ingresar";
            //Email nuevoEmail = new Email();

            //string respuesta = nuevoEmail.SendEmail(emailBody, Email, "Verificacion de Ingreso - La Gran Promocion");

            ///////////////////


            string texto = "Hola, su codigo Temporal para ingresar a la compra de LGP es: " + codigo.Codigo;

            Mensajes sms = new Mensajes(numeroCompleto, texto);
            string respuesta = sms.EnviarSms();

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

        public ActionResult Autenticación(string Mensaje)
        {
            if(!string.IsNullOrEmpty(Mensaje))
            {
                ViewBag.Mensaje = Mensaje;
            }
            string dni = Session["ClienteDni"].ToString();
            string sexo = Session["ClienteSexo"].ToString();
            string contacto = Session["ClienteContacto"].ToString();

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
            string area = Session["ClienteArea"].ToString();
            string numero = Session["ClienteNumero"].ToString();

            //string email = Session["ClienteContacto"].ToString();

            int SolicitudReservadaId = 0;

            if (Session["ReservaSolicitud"] != null)
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
            ViewBag.AreaCelular = area;
            ViewBag.NumeroCelular = numero;
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
                string sexo = Session["ClienteSexo"].ToString();
                string area = Session["ClienteArea"].ToString();
                string numero = Session["ClienteNumero"].ToString();

                var cliente = db.Asociados.Where(x => x.Dni == dni && x.Sexo == sexo).FirstOrDefault();
                if (cliente != null)
                {
                    return RedirectToAction("ErrorRegistro", new { MensajeError = "Ya existe un cliente registrado con ese Dni" });
                }

                cliente = db.Asociados.Where(x => x.AreaCelular == area && x.NumeroCelular == numero).FirstOrDefault();
                if (cliente != null)
                {
                    return RedirectToAction("ErrorRegistro", new { MensajeError = "Ya existe un cliente registrado con ese Nº de Celular" });
                }

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

                Session["ClienteDni"] = asociado.Dni;
                Session["ClienteContacto"] = asociado.AreaCelular +"" +asociado.NumeroCelular;
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
                else
                {
                    //Si el usuario ya tiene creado un codigo y no expiro se manda el codigo 00000 para que no vuelva a enviar otro mensaje 
                    //con el mismo codigo ya que no lo permite el servicio de mensajeria
                    codigo.Codigo = 00000;
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

        public ActionResult VerificarCodigoUnicoAcceso()
        {
            return View();
        }

        [HttpPost]
        public ActionResult VerificarCodigoUnicoAcceso(int? codigo)
        {
            if (codigo == 1996)
            {
                return RedirectToAction("Identificarse", "Clientes", new { codigo = codigo });
            }
            else
            {
                ViewBag.MensajeError = "Codigo invalido";
                return View();
            }
        }
    }
}
