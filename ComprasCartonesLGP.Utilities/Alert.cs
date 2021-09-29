using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComprasCartonesLGP.Utilities
{
    public class Alert
    {
        public string Type { get; set; }
        public string Message { get; set; }
        public bool Dismissible { get; set; }

        public Alert(string _type, string _message, bool _dismissible)
        {
            Type = _type;
            Message = _message;
            Dismissible = _dismissible;
        }
    }
}
