using ComprasCartonesLGP.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComprasCartonesLGP.Dal
{
    public class LGPContext : DbContext
    {
        public LGPContext() : base("DefaultConnection")
        { }

        public virtual DbSet<Asociado> Asociados { get; set; }

        public virtual DbSet<CodigoAcceso> CodigosAccesos { get; set; }
    }
}
