namespace CamelDotNet.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addStateFileNameLimitLineMax : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PerConfig", "LimitLineMax", c => c.Decimal(precision: 18, scale: 2));
            AddColumn("dbo.TestItemConfig", "StateFileName", c => c.String(maxLength: 100));
        }
        
        public override void Down()
        {
            DropColumn("dbo.TestItemConfig", "StateFileName");
            DropColumn("dbo.PerConfig", "LimitLineMax");
        }
    }
}
