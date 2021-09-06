namespace ComprasCartonesLGP.Dal.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class cambiosCompraSolicitud : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Asociado", "Torre", c => c.String());
            AddColumn("dbo.CompraDeSolicitud", "AsociadoID", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.CompraDeSolicitud", "AsociadoID");
            DropColumn("dbo.Asociado", "Torre");
        }
    }
}
