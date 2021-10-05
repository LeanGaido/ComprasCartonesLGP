using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComprasCartonesLGP.Entities
{
    public class CompraDeSolicitud
    {
        public int ID { get; set; }

        public int AsociadoID { get; set; }

        public string NroAsociado { get; set; }

        [NotMapped]
        public virtual Asociado Asociado { get; set; }

        public int SolicitudID { get; set; }

        public string NroSolicitud { get; set; }

        [NotMapped]
        public virtual Solicitud Solicitud { get; set; }

        public DateTime FechaVenta { get; set; }

        [NotMapped]
        public int DiasDesdeLaVenta { get; set; }

        [ForeignKey("TipoDePago")]
        public int TipoDePagoID { get; set; }

        public virtual TipoDePago TipoDePago { get; set; }

        public float TotalAPagar { get; set; }

        public int? CantCuotas { get; set; }

        public int? EntidadID { get; set; }

        [NotMapped]
        public string CheckoutUrl { get; set; }

        public bool PagoRealizdo { get; set; }

        public decimal PagoRealizado { get; set; }

        public DateTime? FechaPago { get; set; }

        //public int AvisosDeuda { get; set; }

        public bool PagoCancelado { get; set; }

        public DateTime? FechaCancelado { get; set; }

        public int CodigoVendedor { get; set; }
    }
}
