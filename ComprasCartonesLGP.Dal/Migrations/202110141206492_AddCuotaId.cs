namespace ComprasCartonesLGP.Dal.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddCuotaId : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.DebitoCard", "CuotaId", c => c.Int(nullable: false));
            AddColumn("dbo.DebitoCBU", "CuotaId", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.DebitoCBU", "CuotaId");
            DropColumn("dbo.DebitoCard", "CuotaId");
        }
    }
}
