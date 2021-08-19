using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComprasCartonesLGP.Entities.Pago360.Request
{
    public class AdhesionCbuPago360Request
    {
        public string adhesion_holder_name { get; set; }//adhesion_holder_name String  SI
        public string email { get; set; }//email String  SI
        public string description { get; set; }//description String  SI
        public string short_description { get; set; }//short_description String  SI
        public string external_reference { get; set; }//external_reference String  SI
        public string cbu_number { get; set; }//cbu_number String  SI
        public long cbu_holder_id_number { get; set; }//cbu_holder_id_number Integer SI
        public string cbu_holder_name { get; set; }//cbu_holder_name String  SI
        //public object metadata { get; set; }//metadata Object  NO
    }
}
