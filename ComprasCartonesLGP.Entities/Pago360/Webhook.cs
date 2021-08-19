using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SueñoCelestePagos.Entities.Pago360
{
    public class Webhook
    {
        [DataMember(Name = "entity_name", EmitDefaultValue = false)]
        public string entity_name { get; set; }
        [DataMember(Name = "type", EmitDefaultValue = false)]
        public string type { get; set; }
        [DataMember(Name = "entity_id", EmitDefaultValue = false)]
        public int entity_id { get; set; }
        [DataMember(Name = "created_at", EmitDefaultValue = false)]
        public string created_at { get; set; }
        [DataMember(Name = "payload", EmitDefaultValue = false)]
        public object payload { get; set; }
    }
}
