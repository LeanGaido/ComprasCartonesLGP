namespace ComprasCartonesLGP.Dal.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class changeFloatXDeci : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.DebitoCBU", "first_total", c => c.Decimal(nullable: false, precision: 18, scale: 2));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.DebitoCBU", "first_total", c => c.Single(nullable: false));
        }
    }
}
