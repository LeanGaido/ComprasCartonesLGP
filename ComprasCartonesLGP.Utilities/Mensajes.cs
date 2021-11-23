using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace ComprasCartonesLGP.Utilities
{
    public class Mensajes
    {
        public string usuario { get; set; }
        public string clave { get; set; }
        public string numero { get; set; }
        public string texto { get; set; }
        public string respuesta { get; set; }

        public Mensajes(string _numero, string _texto)
        {
            usuario = "COSMOPOLITA";//"SMSDEMO766042";//"MIUSUARIO";
            clave = "COSMOPOLITA633";//"SMSDEMO766042108";// "MICLAVE";
            numero = _numero;
            texto = _texto;//"Su codigo Temporal es: " + codigo.Codigo + "\r\nMcBrill";
        }

        public string EnviarSms()
        {
            //Test sin enviar SMS
            //Uri uri = new Uri("http://servicio.smsmasivos.com.ar/enviar_sms.asp?TEST=1&API=1&USUARIO=" + HttpUtility.UrlEncode(usuario) + "&CLAVE=" + HttpUtility.UrlEncode(clave) + "&TOS=" + HttpUtility.UrlEncode(numero) + "&TEXTO=" + HttpUtility.UrlEncode(texto));

            //Test enviando realmente un SMS
            Uri uri = new Uri("http://servicio.smsmasivos.com.ar/enviar_sms.asp?API=1&USUARIO=" + HttpUtility.UrlEncode(usuario) + "&CLAVE=" + HttpUtility.UrlEncode(clave) + "&TOS=" + HttpUtility.UrlEncode(numero) + "&TEXTO=" + HttpUtility.UrlEncode(texto));

            HttpWebRequest requestFile = (HttpWebRequest)WebRequest.Create(uri);

            requestFile.ContentType = "application/html";

            HttpWebResponse webResp = requestFile.GetResponse() as HttpWebResponse;

            if (requestFile.HaveResponse)
            {
                if (webResp.StatusCode == HttpStatusCode.OK || webResp.StatusCode == HttpStatusCode.Accepted)
                {
                    StreamReader respReader = new StreamReader(webResp.GetResponseStream(), Encoding.GetEncoding("utf-8"/*"iso-8859-1"*/));

                    respuesta = respReader.ReadToEnd();
                    if(respuesta == "OK\r\n")
                    {
                        respuesta = "Enviado Correctamente";
                    }
                }
            }

            return respuesta;
        }
    }
}
