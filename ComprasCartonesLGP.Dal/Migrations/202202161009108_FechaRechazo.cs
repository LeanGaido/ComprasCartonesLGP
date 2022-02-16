namespace ComprasCartonesLGP.Dal.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class FechaRechazo : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.DebitoCard", "fechaRechazo", c => c.DateTime(nullable: false));
            AddColumn("dbo.DebitoCBU", "fechaRechazo", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.DebitoCBU", "fechaRechazo");
            DropColumn("dbo.DebitoCard", "fechaRechazo");
        }
    }
}
