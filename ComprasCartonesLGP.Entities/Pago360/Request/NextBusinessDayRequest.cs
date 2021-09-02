using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComprasCartonesLGP.Entities.Pago360.Request
{
    public class NextBusinessDayRequest
    {
        public string date { get; set; }
        public int days { get; set; }
    }
}
