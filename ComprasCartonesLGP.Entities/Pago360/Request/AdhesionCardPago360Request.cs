using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComprasCartonesLGP.Entities.Pago360.Request
{
    public class AdhesionCardPago360Request
    {
        public string adhesion_holder_name { get; set; }//adhesion_holder_name String  SI Nombre del titular del servicio(hasta 50 caracteres).
        public string email { get; set; }//email String  SI Email del titular de la tarjeta(hasta 255 caracteres).
        public string description { get; set; }//description String  SI Descripción o concepto de la Adhesión(hasta 255 caracteres).
        public string external_reference { get; set; }//external_reference String  SI Este atributo se puede utilizar como referencia para identificar la Adhesión y sincronizar con tus sistemas de backend el origen de la operación.Algunos valores comúnmente utilizados son: ID de Cliente, DNI, CUIT, ID de venta o Nro.de Factura entre otros (hasta 255 caracteres).
        public string card_number { get; set; }//card_number String  SI Hash en Base64 que contiene la Encriptación del Número de Tarjeta en la que se ejecutarán los débitos automáticos(hasta 19 caracteres).
        public string card_holder_name { get; set; }//card_holder_name String  SI Nombre del titular de la Tarjeta(hasta 255 caracteres).
        //public string metadata { get; set; }//metadata Object  NO
    }
}
