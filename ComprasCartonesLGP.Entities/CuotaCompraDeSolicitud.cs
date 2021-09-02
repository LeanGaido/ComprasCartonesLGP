using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComprasCartonesLGP.Entities
{
    public class CuotaCompraDeSolicitud
    {
        public int ID { get; set; }

        [ForeignKey("CompraDeSolicitud")]
        public int CompraDeSolicitudID { get; set; }

        public string MesCuota { get; set; }

        public string AnioCuota { get; set; }

        public string PeriodoCuota { get { return MesCuota.ToString() + "/" + AnioCuota.ToString(); } }

        public DateTime PrimerVencimiento { get; set; }

        public float PrimerPrecioCuota { get; set; }

        public DateTime SeguntoVencimiento { get; set; }

        public float SeguntoPrecioCuota { get; set; }

        public int TipoPagoID { get; set; }

        public int PagoID { get; set; }

        public bool CuotaPagada { get; set; }

        public DateTime? FechaPago { get; set; }

        public virtual CompraDeSolicitud CompraDeSolicitud { get; set; }

        public virtual TipoDePago TipoDePago { get; set; }
    }
}
