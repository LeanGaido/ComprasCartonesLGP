using System.ComponentModel.DataAnnotations.Schema;

namespace ComprasCartonesLGP.Entities
{
    public class Solicitud
    {
        public int ID { get; set; }

        [ForeignKey("Promocion")]
        public int PromocionId { get; set; }

        public string NroSolicitud { get; set; }

        public string NroCarton { get; set; }

        public float Precio { get; set; }

        public virtual Promocion Promocion { get; set; }
    }
}