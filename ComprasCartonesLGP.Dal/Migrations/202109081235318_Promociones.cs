namespace ComprasCartonesLGP.Dal.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Promociones : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.CompraDeSolicitud", "SolicitudID", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.CompraDeSolicitud", "SolicitudID");
        }
    }
}
