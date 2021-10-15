using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComprasCartonesLGP.Entities.Pago360.Request
{
    public class CbuDebitRequest
    {
        public int adhesion_id { get; set; }//adhesion_id Integer SI
        public string first_due_date { get; set; }//first_due_date Date    SI
        public decimal first_total { get; set; }//first_total Float   SI
        public string second_due_date { get; set; }//second_due_date Date    NO
        public decimal second_total { get; set; }//second_total Float   NO
        public string description { get; set; }//description String NO
        public Metadata metadata { get; set; }//metadata Object  NO
    }
}
