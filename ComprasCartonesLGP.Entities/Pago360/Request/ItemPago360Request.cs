namespace ComprasCartonesLGP.Entities.Pago360.Request
{
    public class ItemPago360Request
    {
        public int quantity { get; set; }//quantity	Integer	NO
        public string description { get; set; }//description	String	SI
        public float amount { get; set; }//amount	Float	SI
    }
}