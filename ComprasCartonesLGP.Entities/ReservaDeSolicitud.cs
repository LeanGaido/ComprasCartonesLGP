using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComprasCartonesLGP.Entities
{
    public class ReservaDeSolicitud
    {
        public int ID { get; set; }

        public string Dni { get; set; }

        public string Sexo { get; set; }

        [ForeignKey("Solicitud")]
        public int SolicitudID { get; set; }

        public virtual Solicitud Solicitud { get; set; }

        public DateTime FechaReserva { get; set; }

        public DateTime FechaExpiracionReserva { get; set; }

        public int CodigoVendedor { get; set; }
    }
}
