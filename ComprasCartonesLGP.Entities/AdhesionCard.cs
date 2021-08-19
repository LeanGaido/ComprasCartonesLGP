using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComprasCartonesLGP.Entities
{
    public class AdhesionCard
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int id { get; set; }//id Integer ID de Adhesión.

        public string external_reference { get; set; }//external_reference String  Este atributo se puede utilizar como referencia para identificar la Adhesión y sincronizar con tus sistemas de backend el origen de la operación.Algunos valores comúnmente utilizados son: ID de Cliente, DNI, CUIT, ID de venta o Nro.de Factura entre otros.

        public string adhesion_holder_name { get; set; }//adhesion_holder_name    String Nombre del titular del servicio.

        public string email { get; set; }//email String  Email del titular de la Tarjeta.

        public string card_holder_name { get; set; }//card_holder_name String  Nombre del titular de la Tarjeta.

        public string last_four_digits { get; set; }//last_four_digits String  Ultimos 4 numeros de la Tarjeta.

        public string card { get; set; }//card String  Marca de la Tarjeta.

        public string description { get; set; }//description String  Descripción o concepto de la Adhesión.

        public string state { get; set; }//state String  Estado de la Adhesión.

        public DateTime created_at { get; set; }//created_at DateTime    Fecha y hora de creación.

    }
}
