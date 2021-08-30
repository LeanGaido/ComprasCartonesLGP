using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComprasCartonesLGP.Entities
{
    public class DebitoCard
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int id { get; set; } //id	Integer	ID del resultado de la Solicitud de Débito.
        public string type { get; set; }//type	String	Tipo de Solicitud.
        public string state { get; set; }//state	String	Estado de la Solicitud de Débito. .
        public float amount { get; set; }//amount	Float	Importe a ser Debitado. Formato: 00000000.00 (hasta 8 enteros y 2 decimales, utilizando punto “.” como separador decimal).
        public int month { get; set; }//month	Integer	Mes en el que se ejecuta el Debito Automático. Formato: mm.
        public int year { get; set; } //year	Integer	Año en el que se ejecuta el Debito Automático. Formato: aaaa.
        public string description { get; set; }//description	String	Descripción o concepto de la Solicitud de Débito.
        //metadata	Object	Objeto JSON que se puede utilizar para guardar atributos adicionales en la Solicitud de Débito y poder sincronizar con tus sistemas de backend. Pagos360.com no utiliza este objeto.
        [ForeignKey("adhesion")]
        public int adhesionId { get; set; }
        public virtual AdhesionCbu adhesion { get; set; }//card_adhesion	Object	Objeto con el detalle de la Adhesión en Tarjeta.
    }
}
