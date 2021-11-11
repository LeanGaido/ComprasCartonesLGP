using ComprasCartonesLGP.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComprasCartonesLGP.Dal
{
    public class LGPContext : DbContext
    {
        public LGPContext() : base("DefaultConnection")
        { }

        public virtual DbSet<AdhesionCard> AdhesionCard { get; set; }

        public virtual DbSet<AdhesionCbu> AdhesionCbu { get; set; }

        public virtual DbSet<Asociado> Asociados { get; set; }

        public virtual DbSet<CodigoAcceso> CodigosAccesos { get; set; }

        public virtual DbSet<CompraDeSolicitud> ComprasDeSolicitudes { get; set; }

        public virtual DbSet<CuotaCompraDeSolicitud> CuotasCompraDeSolicitudes { get; set; }

        public virtual DbSet<Localidades> Localidades { get; set; }

        public virtual DbSet<Pago> Pagos { get; set; }

        public virtual DbSet<Promocion> Promociones { get; set; }

        public virtual DbSet<Provincias> Provincias { get; set; }

        public virtual DbSet<ReservaDeSolicitud> ReservaDeSolicitudes { get; set; }

        public virtual DbSet<Solicitud> Solicitudes { get; set; }

        public virtual DbSet<TipoDePago> TiposDePagos { get; set; }

        public virtual DbSet<DebitoCard> DebitosCard { get; set; }

        public virtual DbSet<DebitoCBU> DebitosCBU { get; set; }
        public virtual DbSet<Parametro> Parametros { get; set; }


        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            //DONT DO THIS ANYMORE
            //base.OnModelCreating(modelBuilder);
            //modelBuilder.Entity<Vote>().ToTable("Votes")
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
        }
    }
}
