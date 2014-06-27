namespace CamelDotNet.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _201406251345_add_QualityLoss_QualityLossPercent : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.QualityLoss",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TestItemId = c.Int(nullable: false),
                        ProcessId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Process", t => t.ProcessId)
                .ForeignKey("dbo.TestItem", t => t.TestItemId)
                .Index(t => t.ProcessId)
                .Index(t => t.TestItemId);
            
            CreateTable(
                "dbo.QualityLossPercent",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        QualityLossId = c.Int(nullable: false),
                        QualityLossFreq = c.String(),
                        QualityLossRef = c.String(),
                        LossValue = c.Decimal(nullable: false, precision: 6, scale: 2),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.QualityLoss", t => t.QualityLossId)
                .Index(t => t.QualityLossId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.QualityLoss", "TestItemId", "dbo.TestItem");
            DropForeignKey("dbo.QualityLossPercent", "QualityLossId", "dbo.QualityLoss");
            DropForeignKey("dbo.QualityLoss", "ProcessId", "dbo.Process");
            DropIndex("dbo.QualityLoss", new[] { "TestItemId" });
            DropIndex("dbo.QualityLossPercent", new[] { "QualityLossId" });
            DropIndex("dbo.QualityLoss", new[] { "ProcessId" });
            DropTable("dbo.QualityLossPercent");
            DropTable("dbo.QualityLoss");
        }
    }
}
