namespace ComprasCartonesLGP.Dal.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class nullDatetime : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.DebitoCard", "fechaRechazo", c => c.DateTime());
            AlterColumn("dbo.DebitoCBU", "fechaRechazo", c => c.DateTime());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.DebitoCBU", "fechaRechazo", c => c.DateTime(nullable: false));
            AlterColumn("dbo.DebitoCard", "fechaRechazo", c => c.DateTime(nullable: false));
        }
    }
}
