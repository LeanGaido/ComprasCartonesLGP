using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComprasCartonesLGP.Entities.Pago360.Request
{
    public class CardDebitRequest
    {
        public int card_adhesion_id { get; set; }//card_adhesion_id Integer SI ID de la Adhesión asociada a la Solicitud de Débito en Tarjeta.
        public int month { get; set; }//month Integer SI Mes en el que se ejecuta el Debito Automático.Formato: mm.
        public int year { get; set; }//year Integer SI Año en el que se ejecuta el Debito Automático. Formato: aaaa.
        public float amount { get; set; }//amount Float   SI Importe a ser Debitado Automáticamente. Formato: 00000000.00 (hasta 8 enteros y 2 decimales, utilizando punto “.” como separador decimal).
        public string description { get; set; }//description String  NO Descripción o concepto de la Solicitud de Débito(hasta 255 caracteres).
        //public object metadata { get; set; }//metadata Object  NO Objeto JSON que se puede utilizar para guardar atributos adicionales en la Solicitud de Débito y poder sincronizar con tus sistemas de backend.Pagos360.com no utiliza este objeto.
    }
}
