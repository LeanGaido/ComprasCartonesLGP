using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComprasCartonesLGP.Entities.ViewsModels
{
    public class RechazoDebitoVm
    {
        public int Id { get; set; }
        public string NroSolicitud { get; set; }
        public string NombreAsociado { get; set; }
        public string MesCuota { get; set; }
        public DateTime? FechaCreacion { get; set; }
        public DateTime? FechaRechazo { get; set; }
    }
}
