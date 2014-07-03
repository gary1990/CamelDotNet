namespace CamelDotNet.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _201406301652_add_ProductType_Price : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ProductType", "Price", c => c.Decimal(nullable: false, precision: 8, scale: 2));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ProductType", "Price");
        }
    }
}
