namespace CamelDotNet.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class update_QualitylossPercent_precision : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.QualityLossPercent", "LossValue", c => c.Decimal(nullable: false, precision: 11, scale: 5));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.QualityLossPercent", "LossValue", c => c.Decimal(nullable: false, precision: 6, scale: 2));
        }
    }
}
