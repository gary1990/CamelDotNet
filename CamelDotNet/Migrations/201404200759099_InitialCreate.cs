namespace CamelDotNet.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.CamelRole",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 255),
                        IsDeleted = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Permission",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ControllerName = c.String(nullable: false, maxLength: 256),
                        ActionName = c.String(nullable: false, maxLength: 256),
                        Name = c.String(nullable: false, maxLength: 255),
                        IsDeleted = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Client",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 255),
                        IsDeleted = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Department",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 255),
                        IsDeleted = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.PerConfig",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TestItemConfigId = c.Int(nullable: false),
                        Channel = c.Int(nullable: false),
                        Trace = c.Int(nullable: false),
                        StartF = c.Decimal(nullable: false, precision: 11, scale: 5),
                        StartUnitId = c.Int(nullable: false),
                        StopF = c.Decimal(nullable: false, precision: 11, scale: 5),
                        StopUnitId = c.Int(nullable: false),
                        ScanPoint = c.Int(nullable: false),
                        ScanTime = c.Decimal(nullable: false, precision: 11, scale: 5),
                        TransportSpeed = c.Decimal(precision: 11, scale: 5),
                        FreqPoint = c.Decimal(precision: 11, scale: 5),
                        LimitLine = c.Decimal(precision: 11, scale: 5),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Unit", t => t.StartUnitId)
                .ForeignKey("dbo.Unit", t => t.StopUnitId)
                .ForeignKey("dbo.TestItemConfig", t => t.TestItemConfigId)
                .Index(t => t.StartUnitId)
                .Index(t => t.StopUnitId)
                .Index(t => t.TestItemConfigId);
            
            CreateTable(
                "dbo.Unit",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.TestItemConfig",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TestConfigId = c.Int(nullable: false),
                        TestItemId = c.Int(nullable: false),
                        VersionDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.TestConfig", t => t.TestConfigId)
                .ForeignKey("dbo.TestItem", t => t.TestItemId)
                .Index(t => t.TestConfigId)
                .Index(t => t.TestItemId);
            
            CreateTable(
                "dbo.TestConfig",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ProductTypeId = c.Int(nullable: false),
                        IsDeleted = c.Boolean(nullable: false),
                        ClientId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Client", t => t.ClientId)
                .ForeignKey("dbo.ProductType", t => t.ProductTypeId)
                .Index(t => t.ClientId)
                .Index(t => t.ProductTypeId);
            
            CreateTable(
                "dbo.ProductType",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Knumber = c.String(nullable: false, maxLength: 80),
                        ModelName = c.String(maxLength: 255),
                        isLocal = c.Boolean(nullable: false),
                        Name = c.String(nullable: false, maxLength: 255),
                        IsDeleted = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.TestItem",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TestItemCategoryId = c.Int(nullable: false),
                        Formular = c.String(),
                        Name = c.String(nullable: false, maxLength: 255),
                        IsDeleted = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.TestItemCategory", t => t.TestItemCategoryId)
                .Index(t => t.TestItemCategoryId);
            
            CreateTable(
                "dbo.TestItemCategory",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 255),
                        IsDeleted = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Process",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 255),
                        IsDeleted = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.TestStation",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ProcessId = c.Int(nullable: false),
                        Name = c.String(nullable: false, maxLength: 255),
                        IsDeleted = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Process", t => t.ProcessId)
                .Index(t => t.ProcessId);
            
            CreateTable(
                "dbo.QualityPassRecord",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        VnaRecordId = c.Int(nullable: false),
                        QeId = c.String(maxLength: 128),
                        QeSuggestion = c.String(),
                        DepartmentId = c.Int(),
                        TechnologistId = c.String(maxLength: 128),
                        TechnologistSuggestion = c.String(),
                        QmId = c.String(maxLength: 128),
                        QmSuggestion = c.String(),
                        HmId = c.String(maxLength: 128),
                        HmSuggestion = c.String(),
                        NeedHmApprove = c.Boolean(nullable: false),
                        ChangePass = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Department", t => t.DepartmentId)
                .ForeignKey("dbo.CamelUser", t => t.HmId)
                .ForeignKey("dbo.CamelUser", t => t.QeId)
                .ForeignKey("dbo.CamelUser", t => t.QmId)
                .ForeignKey("dbo.CamelUser", t => t.TechnologistId)
                .ForeignKey("dbo.VnaRecord", t => t.VnaRecordId)
                .Index(t => t.DepartmentId)
                .Index(t => t.HmId)
                .Index(t => t.QeId)
                .Index(t => t.QmId)
                .Index(t => t.TechnologistId)
                .Index(t => t.VnaRecordId);
            
            CreateTable(
                "dbo.AspNetUsers",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        UserName = c.String(),
                        PasswordHash = c.String(),
                        SecurityStamp = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.AspNetUserClaims",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ClaimType = c.String(),
                        ClaimValue = c.String(),
                        User_Id = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.User_Id)
                .Index(t => t.User_Id);
            
            CreateTable(
                "dbo.AspNetUserLogins",
                c => new
                    {
                        UserId = c.String(nullable: false, maxLength: 128),
                        LoginProvider = c.String(nullable: false, maxLength: 128),
                        ProviderKey = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.UserId, t.LoginProvider, t.ProviderKey })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.AspNetUserRoles",
                c => new
                    {
                        UserId = c.String(nullable: false, maxLength: 128),
                        RoleId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.UserId, t.RoleId })
                .ForeignKey("dbo.AspNetRoles", t => t.RoleId)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId)
                .Index(t => t.RoleId)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.AspNetRoles",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.VnaRecord",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        SerialNumberId = c.Int(nullable: false),
                        ProductTypeId = c.Int(nullable: false),
                        CamelDotNetUserId = c.String(nullable: false, maxLength: 128),
                        TestEquipmentId = c.Int(nullable: false),
                        TestStationId = c.Int(nullable: false),
                        OrderNumber = c.String(nullable: false),
                        OrderNo = c.String(nullable: false),
                        DrillingCrew = c.String(nullable: false),
                        ReelNumber = c.String(nullable: false),
                        WorkGroup = c.String(nullable: false),
                        TestTime = c.DateTime(nullable: false),
                        TestResult = c.Boolean(nullable: false),
                        Temperature = c.Decimal(nullable: false, precision: 18, scale: 2),
                        InnerLength = c.Decimal(nullable: false, precision: 18, scale: 2),
                        OuterLength = c.Decimal(nullable: false, precision: 18, scale: 2),
                        ClientId = c.Int(nullable: false),
                        isGreenLight = c.Boolean(nullable: false),
                        NoStatistics = c.Boolean(nullable: false),
                        reTest = c.Boolean(nullable: false),
                        Remark = c.String(),
                        BarCodeUsed = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.CamelUser", t => t.CamelDotNetUserId)
                .ForeignKey("dbo.Client", t => t.ClientId)
                .ForeignKey("dbo.ProductType", t => t.ProductTypeId)
                .ForeignKey("dbo.SerialNumber", t => t.SerialNumberId)
                .ForeignKey("dbo.TestEquipment", t => t.TestEquipmentId)
                .ForeignKey("dbo.TestStation", t => t.TestStationId)
                .Index(t => t.CamelDotNetUserId)
                .Index(t => t.ClientId)
                .Index(t => t.ProductTypeId)
                .Index(t => t.SerialNumberId)
                .Index(t => t.TestEquipmentId)
                .Index(t => t.TestStationId);
            
            CreateTable(
                "dbo.SerialNumber",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Number = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.TestEquipment",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Serialnumber = c.String(nullable: false, maxLength: 50),
                        Name = c.String(nullable: false, maxLength: 255),
                        IsDeleted = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.VnaTestItemRecord",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        VnaRecordId = c.Int(nullable: false),
                        TestItemId = c.Int(nullable: false),
                        TestItemResult = c.Boolean(nullable: false),
                        ImagePath = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.TestItem", t => t.TestItemId)
                .ForeignKey("dbo.VnaRecord", t => t.VnaRecordId)
                .Index(t => t.TestItemId)
                .Index(t => t.VnaRecordId);
            
            CreateTable(
                "dbo.VnaTestItemPerRecord",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        VnaTestItemRecordId = c.Int(nullable: false),
                        XValue = c.Decimal(precision: 30, scale: 15),
                        YValue = c.Decimal(precision: 30, scale: 15),
                        RValue = c.Decimal(precision: 30, scale: 15),
                        TestitemPerResult = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.VnaTestItemRecord", t => t.VnaTestItemRecordId)
                .Index(t => t.VnaTestItemRecordId);
            
            CreateTable(
                "dbo.CamelRolePermission",
                c => new
                    {
                        CamelDotNetRoleId = c.Int(nullable: false),
                        PermissionId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.CamelDotNetRoleId, t.PermissionId })
                .ForeignKey("dbo.CamelRole", t => t.CamelDotNetRoleId)
                .ForeignKey("dbo.Permission", t => t.PermissionId)
                .Index(t => t.CamelDotNetRoleId)
                .Index(t => t.PermissionId);
            
            CreateTable(
                "dbo.CamelUser",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        JobNumber = c.String(nullable: false, maxLength: 20),
                        IsDeleted = c.Boolean(nullable: false),
                        CamelDotNetRoleId = c.Int(nullable: false),
                        DepartmentId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.Id)
                .ForeignKey("dbo.CamelRole", t => t.CamelDotNetRoleId)
                .ForeignKey("dbo.Department", t => t.DepartmentId)
                .Index(t => t.Id)
                .Index(t => t.CamelDotNetRoleId)
                .Index(t => t.DepartmentId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.CamelUser", "DepartmentId", "dbo.Department");
            DropForeignKey("dbo.CamelUser", "CamelDotNetRoleId", "dbo.CamelRole");
            DropForeignKey("dbo.CamelUser", "Id", "dbo.AspNetUsers");
            DropForeignKey("dbo.QualityPassRecord", "VnaRecordId", "dbo.VnaRecord");
            DropForeignKey("dbo.VnaTestItemPerRecord", "VnaTestItemRecordId", "dbo.VnaTestItemRecord");
            DropForeignKey("dbo.VnaTestItemRecord", "VnaRecordId", "dbo.VnaRecord");
            DropForeignKey("dbo.VnaTestItemRecord", "TestItemId", "dbo.TestItem");
            DropForeignKey("dbo.VnaRecord", "TestStationId", "dbo.TestStation");
            DropForeignKey("dbo.VnaRecord", "TestEquipmentId", "dbo.TestEquipment");
            DropForeignKey("dbo.VnaRecord", "SerialNumberId", "dbo.SerialNumber");
            DropForeignKey("dbo.VnaRecord", "ProductTypeId", "dbo.ProductType");
            DropForeignKey("dbo.VnaRecord", "ClientId", "dbo.Client");
            DropForeignKey("dbo.VnaRecord", "CamelDotNetUserId", "dbo.CamelUser");
            DropForeignKey("dbo.QualityPassRecord", "TechnologistId", "dbo.CamelUser");
            DropForeignKey("dbo.QualityPassRecord", "QmId", "dbo.CamelUser");
            DropForeignKey("dbo.QualityPassRecord", "QeId", "dbo.CamelUser");
            DropForeignKey("dbo.QualityPassRecord", "HmId", "dbo.CamelUser");
            DropForeignKey("dbo.AspNetUserClaims", "User_Id", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserRoles", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserRoles", "RoleId", "dbo.AspNetRoles");
            DropForeignKey("dbo.AspNetUserLogins", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.QualityPassRecord", "DepartmentId", "dbo.Department");
            DropForeignKey("dbo.TestStation", "ProcessId", "dbo.Process");
            DropForeignKey("dbo.TestItemConfig", "TestItemId", "dbo.TestItem");
            DropForeignKey("dbo.TestItem", "TestItemCategoryId", "dbo.TestItemCategory");
            DropForeignKey("dbo.TestItemConfig", "TestConfigId", "dbo.TestConfig");
            DropForeignKey("dbo.TestConfig", "ProductTypeId", "dbo.ProductType");
            DropForeignKey("dbo.TestConfig", "ClientId", "dbo.Client");
            DropForeignKey("dbo.PerConfig", "TestItemConfigId", "dbo.TestItemConfig");
            DropForeignKey("dbo.PerConfig", "StopUnitId", "dbo.Unit");
            DropForeignKey("dbo.PerConfig", "StartUnitId", "dbo.Unit");
            DropForeignKey("dbo.CamelRolePermission", "PermissionId", "dbo.Permission");
            DropForeignKey("dbo.CamelRolePermission", "CamelDotNetRoleId", "dbo.CamelRole");
            DropIndex("dbo.CamelUser", new[] { "DepartmentId" });
            DropIndex("dbo.CamelUser", new[] { "CamelDotNetRoleId" });
            DropIndex("dbo.CamelUser", new[] { "Id" });
            DropIndex("dbo.QualityPassRecord", new[] { "VnaRecordId" });
            DropIndex("dbo.VnaTestItemPerRecord", new[] { "VnaTestItemRecordId" });
            DropIndex("dbo.VnaTestItemRecord", new[] { "VnaRecordId" });
            DropIndex("dbo.VnaTestItemRecord", new[] { "TestItemId" });
            DropIndex("dbo.VnaRecord", new[] { "TestStationId" });
            DropIndex("dbo.VnaRecord", new[] { "TestEquipmentId" });
            DropIndex("dbo.VnaRecord", new[] { "SerialNumberId" });
            DropIndex("dbo.VnaRecord", new[] { "ProductTypeId" });
            DropIndex("dbo.VnaRecord", new[] { "ClientId" });
            DropIndex("dbo.VnaRecord", new[] { "CamelDotNetUserId" });
            DropIndex("dbo.QualityPassRecord", new[] { "TechnologistId" });
            DropIndex("dbo.QualityPassRecord", new[] { "QmId" });
            DropIndex("dbo.QualityPassRecord", new[] { "QeId" });
            DropIndex("dbo.QualityPassRecord", new[] { "HmId" });
            DropIndex("dbo.AspNetUserClaims", new[] { "User_Id" });
            DropIndex("dbo.AspNetUserRoles", new[] { "UserId" });
            DropIndex("dbo.AspNetUserRoles", new[] { "RoleId" });
            DropIndex("dbo.AspNetUserLogins", new[] { "UserId" });
            DropIndex("dbo.QualityPassRecord", new[] { "DepartmentId" });
            DropIndex("dbo.TestStation", new[] { "ProcessId" });
            DropIndex("dbo.TestItemConfig", new[] { "TestItemId" });
            DropIndex("dbo.TestItem", new[] { "TestItemCategoryId" });
            DropIndex("dbo.TestItemConfig", new[] { "TestConfigId" });
            DropIndex("dbo.TestConfig", new[] { "ProductTypeId" });
            DropIndex("dbo.TestConfig", new[] { "ClientId" });
            DropIndex("dbo.PerConfig", new[] { "TestItemConfigId" });
            DropIndex("dbo.PerConfig", new[] { "StopUnitId" });
            DropIndex("dbo.PerConfig", new[] { "StartUnitId" });
            DropIndex("dbo.CamelRolePermission", new[] { "PermissionId" });
            DropIndex("dbo.CamelRolePermission", new[] { "CamelDotNetRoleId" });
            DropTable("dbo.CamelUser");
            DropTable("dbo.CamelRolePermission");
            DropTable("dbo.VnaTestItemPerRecord");
            DropTable("dbo.VnaTestItemRecord");
            DropTable("dbo.TestEquipment");
            DropTable("dbo.SerialNumber");
            DropTable("dbo.VnaRecord");
            DropTable("dbo.AspNetRoles");
            DropTable("dbo.AspNetUserRoles");
            DropTable("dbo.AspNetUserLogins");
            DropTable("dbo.AspNetUserClaims");
            DropTable("dbo.AspNetUsers");
            DropTable("dbo.QualityPassRecord");
            DropTable("dbo.TestStation");
            DropTable("dbo.Process");
            DropTable("dbo.TestItemCategory");
            DropTable("dbo.TestItem");
            DropTable("dbo.ProductType");
            DropTable("dbo.TestConfig");
            DropTable("dbo.TestItemConfig");
            DropTable("dbo.Unit");
            DropTable("dbo.PerConfig");
            DropTable("dbo.Department");
            DropTable("dbo.Client");
            DropTable("dbo.Permission");
            DropTable("dbo.CamelRole");
        }
    }
}
