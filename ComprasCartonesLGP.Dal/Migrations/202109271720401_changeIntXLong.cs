namespace ComprasCartonesLGP.Dal.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class changeIntXLong : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.AdhesionCbu", "cbu_holder_id_number", c => c.Long(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.AdhesionCbu", "cbu_holder_id_number", c => c.Int(nullable: false));
        }
    }
}
