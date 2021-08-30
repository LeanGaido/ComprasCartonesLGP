using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComprasCartonesLGP.Entities
{
    [Table("tblPago")]
    public class Pago
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ID { get; set; } //id Integer
        public string type { get; set; } //type String  
        public string state { get; set; } //state String
        public DateTime created_at { get; set; } //created_at  DateTime
        public string external_reference { get; set; }//external_reference String  
        public string payer_name { get; set; } //payer_name String
        public string payer_email { get; set; }//payer_email String
        public string description { get; set; } //description String
        public DateTime first_due_date { get; set; } //first_due_date DateTime
        public decimal first_total { get; set; } //first_total Float
        public DateTime second_due_date { get; set; } //first_due_date DateTime
        public decimal second_total { get; set; } //first_total Float
        public string barcode { get; set; } //barcode String
        public string checkout_url { get; set; } //checkout_url String "https://checkout.pagos360.com/payment-request/9455caf6-36ce-11e9-96fd-fb95450d3057",
        public string barcode_url { get; set; } //barcode_url String "https://admin.pagos360.com/api/payment-request/barcode/9455caf6-36ce-11e9-96fd-fb95450d3057",
        public string pdf_url { get; set; } //pdf_url String "https://admin.pagos360.com/api/payment-request/pdf/9455caf6-36ce-11e9-96fd-fb95450d3057"
        public string excluded_channels { get; set; }//excluded_channels Array [String]  Tipos de Medios de Pago que serán omitidos de las opciones al pagador.Valores posibles: credit_card, debit_card, banelco_pmc, link_pagos , DEBIN y non_banking.

        //public List<ItemPago> items { get; set; }//items Array [Object]

        //[ForeignKey("CartonVendido")]
        //public int CartonVendidoID { get; set; }

        //public virtual CartonVendido CartonVendido { get; set; }
    }
}
