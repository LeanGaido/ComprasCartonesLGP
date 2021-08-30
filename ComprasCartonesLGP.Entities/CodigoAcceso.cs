using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComprasCartonesLGP.Entities
{
    public class CodigoAcceso
    {
        public int ID { get; set; }

        public int Codigo { get; set; }

        public string MedioDeAcceso { get; set; }

        public DateTime Expira { get; set; }
    }
}
