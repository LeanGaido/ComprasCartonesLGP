using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComprasCartonesLGP.Entities
{
    public class DebitoCBU
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int id { get; set; } //id	Integer	ID del resultado de la Solicitud de Débito.
        public string type { get; set; }//type	String	Tipo de Solicitud.
        public string state { get; set; }//state	String	Estado de la Solicitud de Débito. .
        public DateTime created_at { get; set; }//created_at	DateTime	Fecha y hora de creación del resultado. .
        public DateTime first_due_date { get; set; }//first_due_date	DateTime	Fecha de vencimiento de la Solicitud de Débito. .
        public float first_total { get; set; }//first_total	Float	Importe a cobrar. Formato: 00000000.00 (hasta 8 enteros y 2 decimales, utilizando punto “.” como separador decimal).
        //metadata	Object	Objeto JSON que se puede utilizar para guardar atributos adicionales en la Solicitud de Débito y poder sincronizar con tus sistemas de backend. Pagos360.com no utiliza este objeto.
        public string descripcion { get; set; }//description	String	Descripción o concepto de la Solicitud de Débito.
        [ForeignKey("adhesion")]
        public int adhesionId { get; set; }
        public virtual AdhesionCbu adhesion { get; set; }//adhesion	Object	Objeto con el detalle de la Adhesión. .
    }
}
