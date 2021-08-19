using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComprasCartonesLGP.Entities.Pago360.Request
{
    public class Pago360Request
    {
        public string description { get; set; }//description String	SI
        public string first_due_date { get; set; }//first_due_date Date    SI
        public float first_total { get; set; }//first_total Float   SI
        public string payer_name { get; set; }//payer_name String  SI
        public string external_reference { get; set; }//external_reference String  NO
        public string second_due_date { get; set; }//second_due_date Date    NO
        public float second_total { get; set; }//second_total Float   NO
        public string payer_email { get; set; }//payer_email String  NO
        public string back_url_success { get; set; }//back_url_success String  NO
        public string back_url_pending { get; set; }//back_url_pending String  NO
        public string back_url_rejected { get; set; }//back_url_rejected String  NO
        public string[] excluded_channels { get; set; }//excluded_channels Array[String]  NO
        public Object metadata { get; set; }//metadata Object  NO
        public ItemPago360Request[] items { get; set; }//items Array [Object]  NO
    }
}
