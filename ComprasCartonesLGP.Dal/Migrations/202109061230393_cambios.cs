namespace ComprasCartonesLGP.Dal.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class cambios : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Asociado", "AreaTelefonoFijo", c => c.String());
            AddColumn("dbo.Asociado", "NumeroTelefonoFijo", c => c.String());
            DropColumn("dbo.Asociado", "TelefonoFijo");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Asociado", "TelefonoFijo", c => c.String());
            DropColumn("dbo.Asociado", "NumeroTelefonoFijo");
            DropColumn("dbo.Asociado", "AreaTelefonoFijo");
        }
    }
}
