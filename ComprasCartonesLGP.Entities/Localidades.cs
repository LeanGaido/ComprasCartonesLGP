using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComprasCartonesLGP.Entities
{
    public class Localidades
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ID { get; set; }

        public string Descripcion { get; set; }

        [ForeignKey("Provincia")]
        public int ProvinciaID { get; set; }

        public virtual Provincias Provincia { get; set; }
    }
}
