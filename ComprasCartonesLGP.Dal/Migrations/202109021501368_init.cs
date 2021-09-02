namespace ComprasCartonesLGP.Dal.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class init : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.AdhesionCard",
                c => new
                    {
                        id = c.Int(nullable: false),
                        external_reference = c.String(),
                        adhesion_holder_name = c.String(),
                        email = c.String(),
                        card_holder_name = c.String(),
                        last_four_digits = c.String(),
                        card = c.String(),
                        description = c.String(),
                        state = c.String(),
                        created_at = c.DateTime(nullable: false),
                        state_comment = c.String(),
                        canceled_at = c.DateTime(),
                    })
                .PrimaryKey(t => t.id);
            
            CreateTable(
                "dbo.AdhesionCbu",
                c => new
                    {
                        id = c.Int(nullable: false),
                        external_reference = c.String(),
                        adhesion_holder_name = c.String(),
                        email = c.String(),
                        cbu_holder_id_number = c.Int(nullable: false),
                        cbu_holder_name = c.String(),
                        cbu_number = c.String(),
                        bank = c.String(),
                        description = c.String(),
                        short_description = c.String(),
                        state = c.String(),
                        created_at = c.DateTime(nullable: false),
                        state_comment = c.String(),
                        canceled_at = c.DateTime(),
                    })
                .PrimaryKey(t => t.id);
            
            CreateTable(
                "dbo.Asociado",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        NumeroDeAsociado = c.String(),
                        Nombre = c.String(),
                        Apellido = c.String(),
                        FechaNacimiento = c.DateTime(nullable: false),
                        Sexo = c.String(),
                        Cuit = c.String(),
                        Dni = c.String(),
                        Direccion = c.String(),
                        Altura = c.Int(nullable: false),
                        Piso = c.String(),
                        Dpto = c.String(),
                        Barrio = c.String(),
                        LocalidadID = c.Int(nullable: false),
                        TelefonoFijo = c.String(),
                        Email = c.String(),
                        AreaCelular = c.String(),
                        NumeroCelular = c.String(),
                        AreaCelularAux = c.String(),
                        NumeroCelularAux = c.String(),
                        TipoDeAsociado = c.Int(nullable: false),
                        FechaAlta = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.Localidades", t => t.LocalidadID, cascadeDelete: true)
                .Index(t => t.LocalidadID);
            
            CreateTable(
                "dbo.Localidades",
                c => new
                    {
                        ID = c.Int(nullable: false),
                        Descripcion = c.String(),
                        ProvinciaID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.Provincias", t => t.ProvinciaID, cascadeDelete: true)
                .Index(t => t.ProvinciaID);
            
            CreateTable(
                "dbo.Provincias",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Descripcion = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.CodigoAcceso",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Codigo = c.Int(nullable: false),
                        MedioDeAcceso = c.String(),
                        Expira = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.CompraDeSolicitud",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        NroAsociado = c.String(),
                        NroSolicitud = c.String(),
                        FechaVenta = c.DateTime(nullable: false),
                        TipoDePagoID = c.Int(nullable: false),
                        TotalAPagar = c.Single(nullable: false),
                        CantCuotas = c.Int(),
                        EntidadID = c.Int(),
                        PagoRealizdo = c.Boolean(nullable: false),
                        PagoRealizado = c.Decimal(nullable: false, precision: 18, scale: 2),
                        FechaPago = c.DateTime(),
                        PagoCancelado = c.Boolean(nullable: false),
                        FechaCancelado = c.DateTime(),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.TipoDePago", t => t.TipoDePagoID, cascadeDelete: true)
                .Index(t => t.TipoDePagoID);
            
            CreateTable(
                "dbo.TipoDePago",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Descripcion = c.String(),
                        Activo = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.CuotaCompraDeSolicitud",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        CompraDeSolicitudID = c.Int(nullable: false),
                        MesCuota = c.String(),
                        AnioCuota = c.String(),
                        PrimerVencimiento = c.DateTime(nullable: false),
                        PrimerPrecioCuota = c.Single(nullable: false),
                        SeguntoVencimiento = c.DateTime(nullable: false),
                        SeguntoPrecioCuota = c.Single(nullable: false),
                        TipoPagoID = c.Int(nullable: false),
                        PagoID = c.Int(nullable: false),
                        CuotaPagada = c.Boolean(nullable: false),
                        FechaPago = c.DateTime(),
                        TipoDePago_ID = c.Int(),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.CompraDeSolicitud", t => t.CompraDeSolicitudID, cascadeDelete: true)
                .ForeignKey("dbo.TipoDePago", t => t.TipoDePago_ID)
                .Index(t => t.CompraDeSolicitudID)
                .Index(t => t.TipoDePago_ID);
            
            CreateTable(
                "dbo.tblPago",
                c => new
                    {
                        ID = c.Int(nullable: false),
                        type = c.String(),
                        state = c.String(),
                        created_at = c.DateTime(nullable: false),
                        external_reference = c.String(),
                        payer_name = c.String(),
                        payer_email = c.String(),
                        description = c.String(),
                        first_due_date = c.DateTime(nullable: false),
                        first_total = c.Decimal(nullable: false, precision: 18, scale: 2),
                        second_due_date = c.DateTime(nullable: false),
                        second_total = c.Decimal(nullable: false, precision: 18, scale: 2),
                        barcode = c.String(),
                        checkout_url = c.String(),
                        barcode_url = c.String(),
                        pdf_url = c.String(),
                        excluded_channels = c.String(),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.ReservaDeSolicitud",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Dni = c.String(),
                        Sexo = c.String(),
                        SolicitudID = c.Int(nullable: false),
                        FechaReserva = c.DateTime(nullable: false),
                        FechaExpiracionReserva = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.Solicitud", t => t.SolicitudID, cascadeDelete: true)
                .Index(t => t.SolicitudID);
            
            CreateTable(
                "dbo.Solicitud",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        PromocionId = c.Int(nullable: false),
                        NroSolicitud = c.String(),
                        NroCarton = c.String(),
                        Precio = c.Single(nullable: false),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.Promocion", t => t.PromocionId, cascadeDelete: true)
                .Index(t => t.PromocionId);
            
            CreateTable(
                "dbo.Promocion",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Descripcion = c.String(),
                        Anio = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ID);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ReservaDeSolicitud", "SolicitudID", "dbo.Solicitud");
            DropForeignKey("dbo.Solicitud", "PromocionId", "dbo.Promocion");
            DropForeignKey("dbo.CuotaCompraDeSolicitud", "TipoDePago_ID", "dbo.TipoDePago");
            DropForeignKey("dbo.CuotaCompraDeSolicitud", "CompraDeSolicitudID", "dbo.CompraDeSolicitud");
            DropForeignKey("dbo.CompraDeSolicitud", "TipoDePagoID", "dbo.TipoDePago");
            DropForeignKey("dbo.Asociado", "LocalidadID", "dbo.Localidades");
            DropForeignKey("dbo.Localidades", "ProvinciaID", "dbo.Provincias");
            DropIndex("dbo.Solicitud", new[] { "PromocionId" });
            DropIndex("dbo.ReservaDeSolicitud", new[] { "SolicitudID" });
            DropIndex("dbo.CuotaCompraDeSolicitud", new[] { "TipoDePago_ID" });
            DropIndex("dbo.CuotaCompraDeSolicitud", new[] { "CompraDeSolicitudID" });
            DropIndex("dbo.CompraDeSolicitud", new[] { "TipoDePagoID" });
            DropIndex("dbo.Localidades", new[] { "ProvinciaID" });
            DropIndex("dbo.Asociado", new[] { "LocalidadID" });
            DropTable("dbo.Promocion");
            DropTable("dbo.Solicitud");
            DropTable("dbo.ReservaDeSolicitud");
            DropTable("dbo.tblPago");
            DropTable("dbo.CuotaCompraDeSolicitud");
            DropTable("dbo.TipoDePago");
            DropTable("dbo.CompraDeSolicitud");
            DropTable("dbo.CodigoAcceso");
            DropTable("dbo.Provincias");
            DropTable("dbo.Localidades");
            DropTable("dbo.Asociado");
            DropTable("dbo.AdhesionCbu");
            DropTable("dbo.AdhesionCard");
        }
    }
}
