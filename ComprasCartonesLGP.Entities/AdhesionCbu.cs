using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComprasCartonesLGP.Entities
{
    public class AdhesionCbu
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int id { get; set; }//id Integer

        public string external_reference { get; set; }//external_reference String

        public string adhesion_holder_name { get; set; }//adhesion_holder_name    String

        public string email { get; set; }//email String

        public string cbu_holder_name { get; set; }//cbu_holder_name String

        public long cbu_holder_id_number { get; set; } //cbu_holder_id_number Integer

        public string cbu_number { get; set; }//cbu_number String

        public string bank { get; set; } //bank String

        public string description { get; set; }//description String

        public string short_description { get; set; }//short_description String

        public string state { get; set; }//state String

        public DateTime created_at { get; set; }//created_at DateTime
    }
}
