namespace ComprasCartonesLGP.Dal.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddVendedor : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.CompraDeSolicitud", "CodigoVendedor", c => c.Int(nullable: false));
            AddColumn("dbo.ReservaDeSolicitud", "CodigoVendedor", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ReservaDeSolicitud", "CodigoVendedor");
            DropColumn("dbo.CompraDeSolicitud", "CodigoVendedor");
        }
    }
}
