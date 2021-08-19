using System.Runtime.Serialization;

namespace ComprasCartonesLGP.Models.Pago360.Response
{
    public class ItemPago360Response
    {
        public int quantity { get; set; }//quantity Integer
        public string description { get; set; }//description String
        public float amount { get; set; }//amount Float
    }
}
