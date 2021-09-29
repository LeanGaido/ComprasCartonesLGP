namespace ComprasCartonesLGP.Dal.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddTablasDebitos : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.DebitoCard",
                c => new
                    {
                        id = c.Int(nullable: false),
                        type = c.String(),
                        state = c.String(),
                        created_at = c.DateTime(),
                        amount = c.Single(nullable: false),
                        month = c.Int(nullable: false),
                        year = c.Int(nullable: false),
                        description = c.String(),
                        adhesionId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.id)
                .ForeignKey("dbo.AdhesionCbu", t => t.adhesionId, cascadeDelete: true)
                .Index(t => t.adhesionId);
            
            CreateTable(
                "dbo.DebitoCBU",
                c => new
                    {
                        id = c.Int(nullable: false),
                        type = c.String(),
                        state = c.String(),
                        created_at = c.DateTime(nullable: false),
                        first_due_date = c.DateTime(nullable: false),
                        first_total = c.Single(nullable: false),
                        second_due_date = c.String(),
                        second_total = c.Decimal(nullable: false, precision: 18, scale: 2),
                        description = c.String(),
                        adhesionId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.id)
                .ForeignKey("dbo.AdhesionCbu", t => t.adhesionId, cascadeDelete: true)
                .Index(t => t.adhesionId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.DebitoCBU", "adhesionId", "dbo.AdhesionCbu");
            DropForeignKey("dbo.DebitoCard", "adhesionId", "dbo.AdhesionCbu");
            DropIndex("dbo.DebitoCBU", new[] { "adhesionId" });
            DropIndex("dbo.DebitoCard", new[] { "adhesionId" });
            DropTable("dbo.DebitoCBU");
            DropTable("dbo.DebitoCard");
        }
    }
}
