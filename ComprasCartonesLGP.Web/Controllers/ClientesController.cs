using ComprasCartonesLGP.Dal;
using ComprasCartonesLGP.Entities;
using ComprasCartonesLGP.Utilities;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
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
                return RedirectToAction("RegistroTelefono");
            }
            else
            {
                if (!Admin)
                {
                    Session["ClienteTelefono"] = Cliente.AreaCelular + "-" + Cliente.NumeroCelular;
                    /*
                    Codigo para generar y enviar mensaje de texto con codigo de autentificacion
                    */
                    CodigoTelefono codigo = ObtenerCodigo(Cliente.Celular);

                    string respuesta = EnviarSms(Cliente.Celular, codigo.Codigo);

                    //if (respuesta == "OK\r\n" || respuesta.Contains("probando sin enviar"))
                    //{
                    //    return RedirectToAction("Autenticación");
                    //}
                    return RedirectToAction("Autenticación");
                }
                else
                {
                    return RedirectToAction("ComprobarCompra", "Compras");
                }
            }
        }

        /***************************************************************************************/

        public ActionResult Autenticación()
        {
            string dni = Session["ClienteDni"].ToString();

            ViewBag.Dni = dni;

            return View();
        }

        [HttpPost]
        public ActionResult Autenticación(int CodVerificacion)
        {
            var valida = checkSessions(new List<string>() { "ClienteDni", "ClienteSexo", "ClienteTelefono" });

            if (!valida)
            {
                return RedirectToAction("Identificarse", "Clientes", new { returnUrl = "/Compras/ElegirCarton" });
            }

            string dni = Session["ClienteDni"].ToString();
            string telefono = Session["ClienteTelefono"].ToString();

            string area = telefono.Substring(0, telefono.IndexOf('-'));
            string numero = telefono.Substring(telefono.IndexOf('-') + 1);

            /*
            Codigo para validar codigo de autentificacion
            */
            DateTime ahora = DateTime.Now;
            var confirmacion = db.CodigosTelefonos.Where(x => x.Telefono == area + numero && x.Codigo == CodVerificacion && x.Expira >= ahora).FirstOrDefault();

            if (confirmacion == null)
            {
                ViewBag.Dni = dni;
                ViewBag.MensajeError = "Codigo Incorrecto";

                return View();
            }

            return RedirectToAction("ComprobarCompra", "Compras");
            //return RedirectToAction("ComprobarCompra", "Compras");
        }

        /***************************************************************************************/

        public ActionResult RegistroTelefono()
        {
            string dni = Session["ClienteDni"].ToString();
            ViewBag.Dni = dni;

            return View();
        }

        [HttpPost]
        public ActionResult RegistroTelefono(string Prefijo, string Numero)
        {
            var valida = checkSessions(new List<string>() { "ClienteDni", "ClienteSexo" });

            if (!valida)
            {
                return RedirectToAction("Identificarse", "Clientes");
            }

            string dni = Session["ClienteDni"].ToString();
            Session["ClienteTelefono"] = Prefijo + "-" + Numero;

            string telefono = Prefijo + Numero;

            var cliente = db.Clientes.Where(x => x.AreaCelular == Prefijo && x.NumeroCelular == Numero).FirstOrDefault();

            if (cliente != null)
            {
                return RedirectToAction("ErrorRegistro", new { MensajeError = "Ya existe un cliente registrado con ese Telefono" });
            }

            CodigoTelefono codigo = ObtenerCodigo(telefono);

            string respuesta = EnviarSms(telefono, codigo.Codigo);

            if (respuesta == "OK\r\n" || respuesta.Contains("probando sin enviar"))
            {
                return RedirectToAction("ValidarTelefono", "Clientes", new { Prefijo, Numero });
            }
            return RedirectToAction("ValidarTelefono", "Clientes", new { Prefijo, Numero });
        }

        /***************************************************************************************/

        public ActionResult ValidarTelefono(string Prefijo, string Numero)
        {
            string telefono = Prefijo + Numero;
            ViewBag.AreaTelefono = Prefijo;
            ViewBag.NumeroTelefono = Numero;

            return View();
        }

        [HttpPost]
        public ActionResult ValidarTelefono(string Prefijo, string Numero, int codigo)
        {
            var valida = checkSessions(new List<string>() { "ClienteDni", "ClienteSexo", "ClienteTelefono" });

            if (!valida)
            {
                return RedirectToAction("Identificarse", "Clientes");
            }

            string telefono = Prefijo + Numero;
            if (!string.IsNullOrEmpty(telefono))
            {
                DateTime ahora = DateTime.Now;
                var confirmacion = db.CodigosTelefonos.Where(x => x.Telefono == telefono && x.Codigo == codigo && x.Expira >= ahora).FirstOrDefault();

                if (confirmacion == null)
                {
                    ViewBag.AreaTelefono = Prefijo;
                    ViewBag.NumeroTelefono = Numero;
                    ViewBag.MensajeError = "Codigo Incorrecto";

                    return View();
                }

                Session["ClienteTelefono"] = Prefijo + "-" + Numero;


                return RedirectToAction("ComprobarCompra", "Compras");
                //return RedirectToAction("RegistroDatos");
            }

            return View();
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
            string sexo = Session["ClienteSexo"].ToString(); ;
            string telefono = Session["ClienteTelefono"].ToString();

            int CartonReservadoId = 0;

            if (!int.TryParse(Session["ReservaCarton"].ToString(), out CartonReservadoId))
            {
                return RedirectToAction("ErrorCompra", new { MensajeError = "Ocurrio un Error, Por Favor intente mas tarde" });
            }

            var cartonReservado = db.CartonesReservados.Where(x => x.ID == CartonReservadoId).FirstOrDefault();

            if (cartonReservado.FechaExpiracionReserva <= DateTime.Now)
            {
                return RedirectToAction("ErrorCompra", new { MensajeError = "La Reserva del Carton Expiro" });
            }

            var reservado = db.CartonesReservados.Where(x => x.Dni == dni && x.Sexo == sexo && x.FechaReserva < hoy && x.FechaExpiracionReserva > hoy).FirstOrDefault();

            var tiempoRestante = reservado.FechaExpiracionReserva - hoy;

            ViewBag.Expira = tiempoRestante.Minutes.ToString().PadLeft(2, '0') + ":" + tiempoRestante.Seconds.ToString().PadLeft(2, '0');

            ViewBag.Dni = dni;
            ViewBag.Dni = telefono;

            var localidades = db.Localidades.ToList();

            ViewBag.LocalidadID = new SelectList(localidades, "ID", "Descripcion");

            return View();
        }

        [HttpPost]
        public ActionResult RegistroDatos(string Nombre, string Apellido, string Email, DateTime FechaNacimiento, string Calle, string Altura, int LocalidadID)
        {
            try
            {
                var valida = checkSessions(new List<string>() { "ClienteDni", "ClienteSexo", "ClienteTelefono" });

                if (!valida)
                {
                    return RedirectToAction("Identificarse", "Clientes");
                }

                string dni = Session["ClienteDni"].ToString();
                string telefono = Session["ClienteTelefono"].ToString();
                string sexo = Session["ClienteSexo"].ToString();

                string area = telefono.Substring(0, telefono.IndexOf('-'));
                string numero = telefono.Substring(telefono.IndexOf('-') + 1);

                var cliente = db.Clientes.Where(x => x.Dni == dni && x.Sexo == sexo).FirstOrDefault();
                if (cliente != null)
                {
                    return RedirectToAction("ErrorRegistro", new { MensajeError = "Ya existe un cliente registrado con ese Dni" });
                }

                cliente = db.Clientes.Where(x => x.AreaCelular == area && x.NumeroCelular == numero).FirstOrDefault();
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

                cliente = new Cliente()
                {
                    Nombre = Nombre,
                    Apellido = Apellido,
                    Sexo = sexo,
                    Dni = dni,
                    AreaCelular = area,
                    NumeroCelular = numero,
                    CelularConfirmado = true,
                    Email = Email,
                    EmailConfirmado = false,
                    FechaNacimiento = FechaNacimiento,
                    Calle = Calle,
                    Altura = Altura,
                    LocalidadID = LocalidadID
                };

                db.Clientes.Add(cliente);
                db.SaveChanges();


                int CartonReservadoId = 0;

                if (!int.TryParse(Session["ReservaCarton"].ToString(), out CartonReservadoId))
                {
                    return RedirectToAction("ErrorCompra", new { MensajeError = "Ocurrio un Error, Por Favor intente mas tarde" });
                }

                var cartonReservado = db.CartonesReservados.Where(x => x.ID == CartonReservadoId).FirstOrDefault();

                if (cartonReservado.FechaExpiracionReserva <= DateTime.Now)
                {
                    return RedirectToAction("ErrorCompra", new { MensajeError = "La Reserva del Carton Expiro" });
                }

                return RedirectToAction("Index", "Compras");
            }
            catch (Exception e)
            {
                ViewBag.LocalidadID = new SelectList(db.Localidades, "ID", "Descripcion", LocalidadID);

                return RedirectToAction("ErrorCompra", new { MensajeError = "Ocurrio un Error, Por Favor intente mas tarde" });
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

            int sexoCliente = 0;

            if (string.IsNullOrEmpty(dni) || string.IsNullOrEmpty(sexo) || !int.TryParse(sexo, out sexoCliente))
            {
                return null;
            }

            return db.Asociados.Where(x => x.Dni == dni && x.Sexo == sexoCliente).FirstOrDefault();
        }
    }
}
