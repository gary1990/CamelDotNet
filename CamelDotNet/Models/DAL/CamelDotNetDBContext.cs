using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Web;

namespace CamelDotNet.Models.DAL
{
    public class CamelDotNetDBContext : IdentityDbContext<CamelDotNetUser>
    {
        public CamelDotNetDBContext()
            : base("CamelDotNet")
        {
            UserManager = new UserManager<CamelDotNetUser>(new UserStore<CamelDotNetUser>(this));
        }

        public UserManager<CamelDotNetUser> UserManager { get; set; }

        public DbSet<CamelDotNetRole> CamelDotNetRole { get; set; }
        public DbSet<TestEquipment> TestEquipment { get; set; }
        public DbSet<Permission> Permission { get; set; }
        public DbSet<TestItem> TestItem { get; set; }
        public DbSet<Process> Process { get; set; }
        public DbSet<TestStation> TestStation { get; set; }
        public DbSet<Client> Client { get; set; }
        public DbSet<ProductType> ProductType { get; set; }
        public DbSet<TestItemCategory> TestItemCategory { get; set; }
        public DbSet<TestConfig> TestConfig { get; set; }
        public DbSet<TestItemConfig> TestItemConfig { get; set; }
        public DbSet<PerConfig> PerConfig { get; set; }
        public DbSet<SerialNumber> SerialNumber { get; set; }
        public DbSet<VnaRecord> VnaRecord { get; set; }
        public DbSet<VnaTestItemRecord> VnaTestItemRecord { get; set; }
        public DbSet<VnaTestItemPerRecord> VnaTestItemPerRecord { get; set; }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
            modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();
            modelBuilder.Conventions.Remove<ManyToManyCascadeDeleteConvention>();
            modelBuilder.Entity<CamelDotNetUser>().ToTable("CamelUser");
            modelBuilder.Entity<CamelDotNetRole>().ToTable("CamelRole");

            modelBuilder.Entity<CamelDotNetRole>().HasMany(a => a.Permissions).WithMany(b => b.CamelDotNetRoles).Map(c => c.MapLeftKey("CamelDotNetRoleId").MapRightKey("PermissionId").ToTable("CamelRolePermission"));
        }
    }

    public class ProcedureCreation 
    {
        public static void Create(CamelDotNetDBContext context) 
        {
            context.Database.ExecuteSqlCommand("Create UNIQUE INDEX index_Name ON ProductType(Name)");
            context.Database.ExecuteSqlCommand("Create UNIQUE INDEX index_Serialnumber ON TestEquipment(Serialnumber)");
        }
    }

    public class CamelDotNetInitializer : DropCreateDatabaseAlways<CamelDotNetDBContext>
    {
        protected override void Seed(CamelDotNetDBContext db)
        {
            ProcedureCreation.Create(db);

            var permissions = new List<Permission>()
            { 
                new Permission{Name = "主页", ActionName="Index", ControllerName = "Home"},
                new Permission{Name = "测试管理", ActionName="Index", ControllerName = "TestManageHome"},
                new Permission{Name = "产品型号", ActionName="Index", ControllerName = "ProductType"},
                new Permission{Name = "产品型号", ActionName="Index", ControllerName = "ProductType"},
                new Permission{Name = "产品型号-查询", ActionName="Get", ControllerName = "ProductType"},
                new Permission{Name = "产品型号-新增", ActionName="Create", ControllerName = "ProductType"},
                new Permission{Name = "产品型号-新增-保存", ActionName="CreateSave", ControllerName = "ProductType"},
                new Permission{Name = "产品型号-编辑", ActionName="Edit", ControllerName = "ProductType"},
                new Permission{Name = "产品型号-编辑-保存", ActionName="EditSave", ControllerName = "ProductType"},
                new Permission{Name = "产品型号-删除", ActionName="Delete", ControllerName = "ProductType"},
                new Permission{Name = "产品型号-删除-确认", ActionName="DeleteSave", ControllerName = "ProductType"},
                new Permission{Name = "产品型号-恢复", ActionName="Restore", ControllerName = "ProductType"},
                new Permission{Name = "产品型号-恢复-确认", ActionName="RestoreSave", ControllerName = "ProductType"},
                new Permission{Name = "产品型号-详情", ActionName="Details", ControllerName = "ProductType"},
                new Permission{Name = "测试项", ActionName="Index", ControllerName = "TestItem"},
                new Permission{Name = "测试项-查询", ActionName="Get", ControllerName = "TestItem"},
                new Permission{Name = "测试项-新增", ActionName="Create", ControllerName = "TestItem"},
                new Permission{Name = "测试项-新增-保存", ActionName="CreateSave", ControllerName = "TestItem"},
                new Permission{Name = "测试项-编辑", ActionName="Edit", ControllerName = "TestItem"},
                new Permission{Name = "测试项-编辑-保存", ActionName="EditSave", ControllerName = "TestItem"},
                new Permission{Name = "测试项-删除", ActionName="Delete", ControllerName = "TestItem"},
                new Permission{Name = "测试项-删除-确认", ActionName="DeleteSave", ControllerName = "TestItem"},
                new Permission{Name = "测试项-恢复", ActionName="Restore", ControllerName = "TestItem"},
                new Permission{Name = "测试项-恢复-确认", ActionName="RestoreSave", ControllerName = "TestItem"},
                new Permission{Name = "测试项-详情", ActionName="Details", ControllerName = "TestItem"},
                new Permission{Name = "测试站点", ActionName="Index", ControllerName = "TestStation"},
                new Permission{Name = "测试站点-查询", ActionName="Get", ControllerName = "TestStation"},
                new Permission{Name = "测试站点-新增", ActionName="Create", ControllerName = "TestStation"},
                new Permission{Name = "测试站点-新增-保存", ActionName="CreateSave", ControllerName = "TestStation"},
                new Permission{Name = "测试站点-编辑", ActionName="Edit", ControllerName = "TestStation"},
                new Permission{Name = "测试站点-编辑-保存", ActionName="EditSave", ControllerName = "TestStation"},
                new Permission{Name = "测试站点-删除", ActionName="Delete", ControllerName = "TestStation"},
                new Permission{Name = "测试站点-删除-确认", ActionName="DeleteSave", ControllerName = "TestStation"},
                new Permission{Name = "测试站点-恢复", ActionName="Restore", ControllerName = "TestStation"},
                new Permission{Name = "测试站点-恢复-确认", ActionName="RestoreSave", ControllerName = "TestStation"},
                new Permission{Name = "测试站点-详情", ActionName="Details", ControllerName = "TestStation"},
                new Permission{Name = "测试设备", ActionName="Index", ControllerName = "TestEquipment"},
                new Permission{Name = "测试设备-查询", ActionName="Get", ControllerName = "TestEquipment"},
                new Permission{Name = "测试设备-新增", ActionName="Create", ControllerName = "TestEquipment"},
                new Permission{Name = "测试设备-新增-保存", ActionName="CreateSave", ControllerName = "TestEquipment"},
                new Permission{Name = "测试设备-编辑", ActionName="Edit", ControllerName = "TestEquipment"},
                new Permission{Name = "测试设备-编辑-保存", ActionName="EditSave", ControllerName = "TestEquipment"},
                new Permission{Name = "测试设备-删除", ActionName="Delete", ControllerName = "TestEquipment"},
                new Permission{Name = "测试设备-删除-确认", ActionName="DeleteSave", ControllerName = "TestEquipment"},
                new Permission{Name = "测试设备-恢复", ActionName="Restore", ControllerName = "TestEquipment"},
                new Permission{Name = "测试设备-恢复-确认", ActionName="RestoreSave", ControllerName = "TestEquipment"},
                new Permission{Name = "测试设备-详情", ActionName="Details", ControllerName = "TestEquipment"},
                new Permission{Name = "测试方案", ActionName="Index", ControllerName = "TestConfig"},
                new Permission{Name = "测试方案-查询", ActionName="Get", ControllerName = "TestConfig"},
                new Permission{Name = "测试方案-复制", ActionName="Copy", ControllerName = "TestConfig"},
                new Permission{Name = "测试方案-复制-保存", ActionName="CopySave", ControllerName = "TestConfig"},
                new Permission{Name = "测试方案-新增", ActionName="Create", ControllerName = "TestConfig"},
                new Permission{Name = "测试方案-新增-保存", ActionName="CreateSave", ControllerName = "TestConfig"},
                new Permission{Name = "测试方案-编辑", ActionName="Edit", ControllerName = "TestConfig"},
                new Permission{Name = "测试方案-编辑-保存", ActionName="EditSave", ControllerName = "TestConfig"},
                new Permission{Name = "测试方案-删除", ActionName="Delete", ControllerName = "TestConfig"},
                new Permission{Name = "测试方案-删除-确认", ActionName="DeleteSave", ControllerName = "TestConfig"},
                new Permission{Name = "测试方案-恢复", ActionName="Restore", ControllerName = "TestConfig"},
                new Permission{Name = "测试方案-恢复-确认", ActionName="RestoreSave", ControllerName = "TestConfig"},
                new Permission{Name = "测试方案-详情", ActionName="Details", ControllerName = "TestConfig"},
                new Permission{Name = "客户", ActionName="Index", ControllerName = "Client"},
                new Permission{Name = "客户-查询", ActionName="Get", ControllerName = "Client"},
                new Permission{Name = "客户-新增", ActionName="Create", ControllerName = "Client"},
                new Permission{Name = "客户-新增-保存", ActionName="CreateSave", ControllerName = "Client"},
                new Permission{Name = "客户-编辑", ActionName="Edit", ControllerName = "Client"},
                new Permission{Name = "客户-编辑-保存", ActionName="EditSave", ControllerName = "Client"},
                new Permission{Name = "客户-删除", ActionName="Delete", ControllerName = "Client"},
                new Permission{Name = "客户-删除-确认", ActionName="DeleteSave", ControllerName = "Client"},
                new Permission{Name = "客户-恢复", ActionName="Restore", ControllerName = "Client"},
                new Permission{Name = "客户-恢复-确认", ActionName="RestoreSave", ControllerName = "Client"},
                new Permission{Name = "客户-详情", ActionName="Details", ControllerName = "Client"},

                new Permission{Name = "质量追溯", ActionName="Index", ControllerName = "QualityTracingHome"},
                new Permission{Name = "VNA测试", ActionName="Index", ControllerName = "VnaTestRecord"},
                new Permission{Name = "VNA测试-查询", ActionName="Get", ControllerName = "VnaTestRecord"},

                new Permission{Name = "报表管理", ActionName="Index", ControllerName = "ReportHome"},

                new Permission{Name = "系统管理", ActionName="Index", ControllerName = "SystemHome"},
                new Permission{Name = "用户管理", ActionName="Index", ControllerName = "UserProfile"},
                new Permission{Name = "用户管理-查询", ActionName="Get", ControllerName = "UserProfile"},
                new Permission{Name = "用户管理", ActionName="Index", ControllerName = "UserProfile"},
                new Permission{Name = "用户管理-查询", ActionName="Get", ControllerName = "UserProfile"},
                new Permission{Name = "用户管理-新增", ActionName="Create", ControllerName = "UserProfile"},
                new Permission{Name = "用户管理-新增-保存", ActionName="CreateSave", ControllerName = "UserProfile"},
                new Permission{Name = "用户管理-编辑", ActionName="Edit", ControllerName = "UserProfile"},
                new Permission{Name = "用户管理-编辑-保存", ActionName="EditSave", ControllerName = "UserProfile"},
                new Permission{Name = "用户管理-删除", ActionName="Delete", ControllerName = "UserProfile"},
                new Permission{Name = "用户管理-删除-确认", ActionName="DeleteSave", ControllerName = "UserProfile"},
                new Permission{Name = "用户管理-恢复", ActionName="Restore", ControllerName = "UserProfile"},
                new Permission{Name = "用户管理-恢复-确认", ActionName="RestoreSave", ControllerName = "UserProfile"},
                new Permission{Name = "用户管理-详情", ActionName="Details", ControllerName = "UserProfile"},
                new Permission{Name = "用户管理-重置密码", ActionName="ResetPassword", ControllerName = "UserProfile"},
                new Permission{Name = "用户管理-重置密码-确认", ActionName="ResetPasswordSave", ControllerName = "UserProfile"},
                new Permission{Name = "角色管理", ActionName="Index", ControllerName = "Role"},
                new Permission{Name = "权限管理", ActionName="Index", ControllerName = "Permission"},
                new Permission{Name = "权限管理-查询", ActionName="Get", ControllerName = "Permission"},
                new Permission{Name = "权限管理-编辑", ActionName="Edit", ControllerName = "Permission"},
                new Permission{Name = "权限管理-编辑-保存", ActionName="EditSave", ControllerName = "Permission"},
                new Permission{Name = "权限管理-删除", ActionName="Delete", ControllerName = "Permission"},
                new Permission{Name = "权限管理-删除-确认", ActionName="DeleteSave", ControllerName = "Permission"},
                new Permission{Name = "权限管理-恢复", ActionName="Restore", ControllerName = "Permission"},
                new Permission{Name = "权限管理-恢复-确认", ActionName="RestoreSave", ControllerName = "Permission"},
                new Permission{Name = "角色管理", ActionName="Index", ControllerName = "Role"},
                new Permission{Name = "角色管理-查询", ActionName="Get", ControllerName = "Role"},
                new Permission{Name = "角色管理-编辑", ActionName="Edit", ControllerName = "Role"},
                new Permission{Name = "角色管理-编辑-保存", ActionName="EditSaveWithMultiselect", ControllerName = "Role"},
                new Permission{Name = "角色管理-删除", ActionName="Delete", ControllerName = "Role"},
                new Permission{Name = "角色管理-删除-确认", ActionName="DeleteSave", ControllerName = "Role"},
                new Permission{Name = "角色管理-恢复", ActionName="Restore", ControllerName = "Role"},
                new Permission{Name = "角色管理-恢复-确认", ActionName="RestoreSave", ControllerName = "Role"},
            };
            permissions.ForEach(a => db.Permission.Add(a));
            db.SaveChanges();

            var processes = new List<Process>()
            {
                new Process{Name = "绝缘"},
                new Process{Name = "焊接"},
                new Process{Name = "护套"},
            };
            processes.ForEach(a => db.Process.Add(a));
            db.SaveChanges();

            var testStations = new List<TestStation>()
            {
                new TestStation{Name = "TS1",ProcessId = db.Process.Where(a => a.Name == "绝缘").SingleOrDefault().Id},
                new TestStation{Name = "TS2",ProcessId = db.Process.Where(a => a.Name == "绝缘").SingleOrDefault().Id},
                new TestStation{Name = "TS3",ProcessId = db.Process.Where(a => a.Name == "护套").SingleOrDefault().Id},
                new TestStation{Name = "TS11",ProcessId = db.Process.Where(a => a.Name == "焊接").SingleOrDefault().Id},
                new TestStation{Name = "teststation2",ProcessId = db.Process.Where(a => a.Name == "焊接").SingleOrDefault().Id},
            };
            testStations.ForEach(a => db.TestStation.Add(a));
            db.SaveChanges();

            var testEquipments = new List<TestEquipment>()
            {
                new TestEquipment{Name = "TE1", Serialnumber = "MY-123456"},
                new TestEquipment{Name = "TE2", Serialnumber = "MY-908097"},
                new TestEquipment{Name = "TE3", Serialnumber = "MY-668678"},
                new TestEquipment{Name = "TE4", Serialnumber = "MY42100275"},
            };
            testEquipments.ForEach(a => db.TestEquipment.Add(a));
            db.SaveChanges();

            var productTypes = new List<ProductType>()
            {
                new ProductType{Name = "PT1"},
                new ProductType{Name = "PT2"},
                new ProductType{Name = "PT3", IsDeleted = true},
                new ProductType{Name = "PT11"},
                new ProductType{Name = "Prodctype1"},
                new ProductType{Name = "producttype2"},
            };
            productTypes.ForEach(a => db.ProductType.Add(a));
            db.SaveChanges();

            var clients = new List<Client>()
            {
                new Client{Name = "General"},
                new Client{Name = "C1"},
                new Client{Name = "C2"},
                new Client{Name = "C3", IsDeleted = true},
                new Client{Name = "C11"},
                new Client{Name = "Client1"},
            };
            clients.ForEach(a => db.Client.Add(a));
            db.SaveChanges();

            var testItemCategory = new List<TestItemCategory>()
            {
                new TestItemCategory{Name = "电器性能"},
                new TestItemCategory{Name = "非电器性能"},
            };
            testItemCategory.ForEach(a => db.TestItemCategory.Add(a));
            db.SaveChanges();

            var testItems = new List<TestItem>()
            {
                new TestItem{Name = "偏心", TestItemCategoryId = 2 },
                new TestItem{Name = "内导体划伤", TestItemCategoryId = 2 },
                new TestItem{Name = "外径不稳", TestItemCategoryId = 2 },
                new TestItem{Name = "粘附力", TestItemCategoryId = 2 },
                new TestItem{Name = "同心度", TestItemCategoryId = 2 },
                new TestItem{Name = "电容不合格", TestItemCategoryId = 2 },
                new TestItem{Name = "芯线焦料", TestItemCategoryId = 2 },
                new TestItem{Name = "泡孔", TestItemCategoryId = 2 },
                new TestItem{Name = "焊接性能不合格退下", TestItemCategoryId = 2 },
                new TestItem{Name = "凹陷", TestItemCategoryId = 2 },
                new TestItem{Name = "轧纹轴承爆裂", TestItemCategoryId = 2 },
                new TestItem{Name = "外径", TestItemCategoryId = 2 },
                new TestItem{Name = "有小洞", TestItemCategoryId = 2 },
                new TestItem{Name = "撞伤", TestItemCategoryId = 2 },
                new TestItem{Name = "内端进水", TestItemCategoryId = 2 },
                new TestItem{Name = "节距", TestItemCategoryId = 2 },
                new TestItem{Name = "图像异常", TestItemCategoryId = 2 },
                new TestItem{Name = "划痕/划伤", TestItemCategoryId = 2 },
                new TestItem{Name = "起车线", TestItemCategoryId = 2 },
                new TestItem{Name = "轧开", TestItemCategoryId = 2 },
                new TestItem{Name = "粘附物较多", TestItemCategoryId = 2 },
                new TestItem{Name = "芯线竹节", TestItemCategoryId = 2 },
                new TestItem{Name = "绝缘芯线偏心", TestItemCategoryId = 2 },
                new TestItem{Name = "段长不足", TestItemCategoryId = 2 },
                new TestItem{Name = "铜带用错0.2当成0.17", TestItemCategoryId = 2 },
                new TestItem{Name = "内端打折", TestItemCategoryId = 2 },
                new TestItem{Name = "盘具用错", TestItemCategoryId = 2 },
                new TestItem{Name = "螺纹不清", TestItemCategoryId = 2 },
                new TestItem{Name = "排线乱", TestItemCategoryId = 2 },
                new TestItem{Name = "焦料", TestItemCategoryId = 2 },
                new TestItem{Name = "盘具被铲坏", TestItemCategoryId = 2 },
                new TestItem{Name = "盘具中心无铁皮", TestItemCategoryId = 2 },
                new TestItem{Name = "小洞", TestItemCategoryId = 2 },
                new TestItem{Name = "作业卡书写错误", TestItemCategoryId = 2 },
                new TestItem{Name = "偏心", TestItemCategoryId = 2 },
                new TestItem{Name = "刮伤、划痕", TestItemCategoryId = 2 },
                new TestItem{Name = "打印间隔", TestItemCategoryId = 2 },
                new TestItem{Name = "印字", TestItemCategoryId = 2 },
                new TestItem{Name = "23外导体挤护成22", TestItemCategoryId = 2 },
                new TestItem{Name = "铜管用错", TestItemCategoryId = 2 },
                new TestItem{Name = "混料", TestItemCategoryId = 2 },
                new TestItem{Name = "绝缘电容不合格", TestItemCategoryId = 2 },
                new TestItem{Name = "外导体开焊", TestItemCategoryId = 2 },
                new TestItem{Name = "外导体刮伤", TestItemCategoryId = 2 },
                new TestItem{Name = "外导体凹陷", TestItemCategoryId = 2 },
                new TestItem{Name = "线压扁", TestItemCategoryId = 2 },
                new TestItem{Name = "线不整圆", TestItemCategoryId = 2 },
                new TestItem{Name = "竹节", TestItemCategoryId = 2 },
                new TestItem{Name = "护套破损", TestItemCategoryId = 2 },
                new TestItem{Name = "火花机报警", TestItemCategoryId = 2 },
                new TestItem{Name = "抗拉强度", TestItemCategoryId = 2 },
                new TestItem{Name = "短路", TestItemCategoryId = 2 },
                new TestItem{Name = "脱胶", TestItemCategoryId = 2 },
                new TestItem{Name = "外护不良、毛糙", TestItemCategoryId = 2 },
                new TestItem{Name = "段长不合格", TestItemCategoryId = 2 },
                new TestItem{Name = "外护塑化不良", TestItemCategoryId = 2 },
                new TestItem{Name = "气孔", TestItemCategoryId = 2 },
                new TestItem{Name = "间隔性有水", TestItemCategoryId = 2 },
                new TestItem{Name = "伸长率", TestItemCategoryId = 2 },
                new TestItem{Name = "水印", TestItemCategoryId = 2 },
                new TestItem{Name = "氧化", TestItemCategoryId = 2 },
                new TestItem{Name = "包", TestItemCategoryId = 2 },
                new TestItem{Name = "击穿", TestItemCategoryId = 2 },
                new TestItem{Name = "备注", TestItemCategoryId = 2 },
                new TestItem{Name = "驻波1", TestItemCategoryId = 1},
                new TestItem{Name = "驻波2", TestItemCategoryId = 1},
                new TestItem{Name = "衰减", Formular = "A+B = C", TestItemCategoryId = 1},
                new TestItem{Name = "时域阻抗", TestItemCategoryId = 1 },
                new TestItem{Name = "TDR电长度", TestItemCategoryId = 1 },
                new TestItem{Name = "回波损耗1", TestItemCategoryId = 1 },
            };
            testItems.ForEach(a => db.TestItem.Add(a));
            db.SaveChanges();

            var testConfigs = new List<TestConfig>()
            {
                new TestConfig{ClientId = 1, ProductTypeId = 1},
                //new TestConfig{ClientId = 1, ProductTypeId = 2},
                //new TestConfig{ClientId = 1, ProductTypeId = 3},
                //new TestConfig{ClientId = 2, ProductTypeId = 1},
                //new TestConfig{ClientId = 2, ProductTypeId = 2},
                //new TestConfig{ClientId = 2, ProductTypeId = 3},
                //new TestConfig{ClientId = 3, ProductTypeId = 1},
                //new TestConfig{ClientId = 3, ProductTypeId = 2},
                //new TestConfig{ClientId = 3, ProductTypeId = 3},
            };
            testConfigs.ForEach(a => db.TestConfig.Add(a));
            db.SaveChanges();

            var testItemConfigs = new List<TestItemConfig>()
            {
                new TestItemConfig{ TestConfigId = 1, TestItemId = 1, VersionDate = DateTime.Now},
                //new TestItemConfig{ TestConfigId = 1, TestItemId = 2, VersionDate = DateTime.Now},
            };
            testItemConfigs.ForEach(a => db.TestItemConfig.Add(a));
            db.SaveChanges();

            var perConfigs = new List<PerConfig>() 
            {
                new PerConfig{ TestItemConfigId = 1, Channel = 1, Trace = 1, ScanPoint = 1, ScanTime = 1, StartF = 1, StopF = 1, TransportSpeed = 1, LimitLine = 1},
                new PerConfig{ TestItemConfigId = 1, Channel = 11, Trace = 11, ScanPoint = 11, ScanTime = 11, StartF = 11, StopF = 11, LimitLine = 11},
                new PerConfig{ TestItemConfigId = 1, Channel = 111, Trace = 111, ScanPoint = 111, StartF = 111, StopF = 111, TransportSpeed = 111, LimitLine = 111},
                //new PerConfig{ TestItemConfigId = 2, Channel = 2, Trace = 2, ScanPoint = 2, ScanTime = 2, StartF = 2, StopF = 2, TransportSpeed = 2, LimitLine = 2},
                //new PerConfig{ TestItemConfigId = 2, Channel = 22, Trace = 22, ScanPoint = 22, ScanTime = 22, StartF = 22, StopF = 22, LimitLine = 22},
                //new PerConfig{ TestItemConfigId = 2, Channel = 222, Trace = 222, ScanPoint = 222, StartF = 222, StopF = 222, TransportSpeed = 222, LimitLine = 222},
            };
            perConfigs.ForEach(a => db.PerConfig.Add(a));
            db.SaveChanges();

            

            var adminRole = new CamelDotNetRole{Name = "Admin"};
            foreach(var item in permissions)
            {
                adminRole.Permissions.Add(item);
            }
            db.CamelDotNetRole.Add(adminRole);
            db.SaveChanges();


            var vnaTesterRole = new CamelDotNetRole { Name = "VnaTester" };
            List<string> vnaTesterPermission = new List<string> { "主页", "测试管理", };
            foreach (var item in permissions)
            {
                if(vnaTesterPermission.Contains(item.Name))
                {
                    vnaTesterRole.Permissions.Add(item);
                }
            }
            db.CamelDotNetRole.Add(vnaTesterRole);
            db.SaveChanges();

            var UserManager = new UserManager<CamelDotNetUser>(new UserStore<CamelDotNetUser>(db));
            var name = "Admin";
            var jobNumber = "GC0001";
            var password = "123456";
            var userAdmin = new CamelDotNetUser();
            userAdmin.UserName = name;
            userAdmin.JobNumber = jobNumber;
            userAdmin.CamelDotNetRole = adminRole;
            UserManager.Create(userAdmin, password);

            name = "VNAT001";
            jobNumber = "no004";
            password = "123456";

            var tester001 = new CamelDotNetUser();
            tester001.UserName = name;
            tester001.JobNumber = jobNumber;
            tester001.CamelDotNetRole = vnaTesterRole;
            UserManager.Create(tester001, password);

            db.SaveChanges();


            var serialNumbers = new List<SerialNumber>() 
            {
                new SerialNumber{ Number = "20140321001"},
                new SerialNumber{ Number = "20140321002"},
                new SerialNumber{ Number = "20140321003"},
            };
            serialNumbers.ForEach(a => db.SerialNumber.Add(a));
            db.SaveChanges();

            var vnaRecords = new List<VnaRecord>()
            {
                new VnaRecord{ SerialNumberId = 1, ProductTypeId = 1, CamelDotNetUserId = UserManager.FindByName("VNAT001").Id, TestTime = new DateTime(2014,03,20,16,28,20), InnerLength = 12.3M, OuterLength = 19.7M, OrderNumber = "123412", DrillingCrew = "34134", Temperature = 18M, TestEquipmentId = 1, TestStationId = 1, ReelNumber = "12343", Remark = "werqr", WorkGroup = "morning"},
                new VnaRecord{ SerialNumberId = 2, ProductTypeId = 1, CamelDotNetUserId = UserManager.FindByName("VNAT001").Id, TestTime = new DateTime(2014,03,21,14,28,20), InnerLength = 11.3M, OuterLength = 18.7M, OrderNumber = "123512", DrillingCrew = "34124", Temperature = 18M, TestEquipmentId = 1, TestStationId = 1, ReelNumber = "12243", Remark = "werqr", WorkGroup = "morning"},
                new VnaRecord{ SerialNumberId = 3, ProductTypeId = 2, CamelDotNetUserId = UserManager.FindByName("Admin").Id, TestTime = new DateTime(2014,03,21,14,28,20), InnerLength = 11.3M, OuterLength = 18.7M, OrderNumber = "123511", DrillingCrew = "341245", Temperature = 18M, TestEquipmentId = 2, TestStationId = 2, ReelNumber = "122435", Remark = "werqr", WorkGroup = "morning",TestResult = true},
            };
            vnaRecords.ForEach(a => db.VnaRecord.Add(a));
            db.SaveChanges();

            base.Seed(db);
        }
    }
}