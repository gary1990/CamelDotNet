namespace CamelDotNet.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addStateFileNameLimitLineMax1 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.PerConfig", "LimitLineMax", c => c.Decimal(precision: 11, scale: 5));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.PerConfig", "LimitLineMax", c => c.Decimal(precision: 18, scale: 2));
        }
    }
}
