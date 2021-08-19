using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace ComprasCartonesLGP.Utilities
{
    public class Email
    {
        private MailMessage mail;
        private SmtpClient SmtpServer;
        private MailAddress From;
        private NetworkCredential cuenta;
        private int puerto;
        private bool autenticacion;

        public Email()
        {
            mail = new MailMessage();
            SmtpServer = new SmtpClient("{host}");
            From = new MailAddress("no-reply@{dominio}.com", "Avisos La Gran Promocion", Encoding.Default);
            cuenta = new NetworkCredential("no-reply@{dominio}.com", "{password}");
            puerto = 587;
            autenticacion = false;
        }

        public Email(MailMessage _mail, SmtpClient _SmtpServer, MailAddress _From, NetworkCredential _cuenta, int _puerto, bool _autenticacion)
        {
            mail = _mail;
            SmtpServer = _SmtpServer;
            From = _From;
            cuenta = _cuenta;
            puerto = _puerto;
            autenticacion = _autenticacion;
        }

        public string SendEmail(string emailBody, string To, string subject)
        {
            try
            {
                mail.From = From;
                mail.IsBodyHtml = true;

                mail.Subject = subject;
                mail.Body = emailBody;
                mail.To.Add(To);

                SmtpServer.Port = puerto;
                SmtpServer.EnableSsl = autenticacion;
                SmtpServer.Credentials = cuenta;

                SmtpServer.Send(mail);
                return "Enviado Correctamente";
            }
            catch (Exception e)
            {
                return e.InnerException.Message.ToString();
            }
        }
    }
}
