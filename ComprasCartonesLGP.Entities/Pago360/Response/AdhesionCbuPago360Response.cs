using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComprasCartonesLGP.Entities.Pago360.Response
{
    public class AdhesionCbuPago360Response
    {
        public int id { get; set; }//id	Integer	ID de Adhesión.
        public string external_reference { get; set; }//external_reference	String	Este atributo se puede utilizar como referencia para identificar la Adhesión y sincronizar con tus sistemas de backend el origen de la operación. Algunos valores comúnmente utilizados son: ID de Cliente, DNI, CUIT, ID de venta o Nro. de Factura entre otros.
        public string adhesion_holder_name { get; set; }//adhesion_holder_name	String	Nombre del titular del servicio que se debitará.
        public string email { get; set; }//email	String	Email del del titular de la cuenta bancaria.
        public long cbu_holder_id_number { get; set; }//cbu_holder_id_number	Integer	CUIT/CUIL del títular de la cuenta bancaria.
        public string cbu_holder_name { get; set; }//cbu_holder_name	String	Nombre del titular de la cuenta bancaria.
        public string cbu_number { get; set; }//cbu_number	String	Número de CBU de la cuenta bancaria en la que se ejecutarán los débitos.
        public string bank { get; set; }//bank	String	Nombre de la entidad bancaria a la que corresponde el número de CBU.
        public string description { get; set; }//description	String	Descripción o concepto de la Adhesión.
        public string short_description { get; set; }//short_description	String	Descripción Bancaria que se mostrará en el resumen de la cuenta bancaria del pagador.
        //metadata	Object	Objeto JSON que se puede utilizar para guardar atributos adicionales en la adhesión y poder sincronizar con tus sistemas de backend. Pagos360.com no utiliza este objeto.
        public string state { get; set; }//state	String	Estado de la Adhesión. .
        public DateTime created_at { get; set; }//created_at	DateTime	Fecha y hora de creación. .
        public string state_comment { get; set; }//state_comment	String	Motivo de cancelación de una Adhesión.
        public DateTime? canceled_at { get; set; }//canceled_at	DateTime	Fecha y hora de cancelación. .

    }
}
