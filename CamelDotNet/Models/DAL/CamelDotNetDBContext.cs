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

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
            modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();
            modelBuilder.Conventions.Remove<ManyToManyCascadeDeleteConvention>();
            modelBuilder.Entity<CamelDotNetUser>().ToTable("CamelUser");
            modelBuilder.Entity<CamelDotNetRole>().ToTable("CamelRole");

            modelBuilder.Entity<CamelDotNetRole>().HasMany(a => a.Permissions).WithMany(b => b.CamelDotNetRoles).Map(c => c.MapLeftKey("CamelDotNetRoleId").MapRightKey("PermissionId").ToTable("CamelRolePermission"));
            //modelBuilder.Entity<TestStation>().Map(a => a.)
        }
    }

    public class CamelDotNetInitializer : DropCreateDatabaseAlways<CamelDotNetDBContext> 
    {
        protected override void Seed(CamelDotNetDBContext db)
        {
            var permissions = new List<Permission>() 
            { 
                new Permission{Name = "主页", ActionName="Index", ControllerName = "Home"},
                new Permission{Name = "测试管理", ActionName="Index", ControllerName = "TestManageHome"},
                new Permission{Name = "产品型号", ActionName="Index", ControllerName = "ProductType"},
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
                new Permission{Name = "报表管理", ActionName="Index", ControllerName = "ReportHome"},
                new Permission{Name = "系统管理", ActionName="Index", ControllerName = "SystemHome"},
                new Permission{Name = "用户管理", ActionName="Index", ControllerName = "UserProfile"},
                new Permission{Name = "权限管理", ActionName="Index", ControllerName = "Permission"},
                new Permission{Name = "角色管理", ActionName="Index", ControllerName = "Role"},
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

            //var testStations = new List<TestStation>()
            //{
            //    //new TestStation{Name = "TS1",ProcessId = db.Process.Where(a => a.Name == "绝缘").SingleOrDefault().Id.ToString()},
            //    //new TestStation{Name = "TS2",ProcessId = db.Process.Where(a => a.Name == "绝缘").SingleOrDefault().Id.ToString()},
            //    //new TestStation{Name = "TS3",ProcessId = db.Process.Where(a => a.Name == "护套").SingleOrDefault().Id.ToString()},
            //    //new TestStation{Name = "TS11",ProcessId = db.Process.Where(a => a.Name == "焊接").SingleOrDefault().Id.ToString()},
            //    new TestStation{Name = "TS1",ProcessId = "1"},
            //    new TestStation{Name = "TS2",ProcessId = "2"},
            //    new TestStation{Name = "TS3",ProcessId = "1"},
            //    new TestStation{Name = "TS11",ProcessId = "3"},
            //};
            //testStations.ForEach(a => db.TestStation.Add(a));
            //db.SaveChanges();

            var testEquipments = new List<TestEquipment>()
            {
                new TestEquipment{Name = "TE1", Serialnumber = "MY-123456"},
                new TestEquipment{Name = "TE2", Serialnumber = "MY-908097"},
                new TestEquipment{Name = "TE3", Serialnumber = "MY-668678"},
                new TestEquipment{Name = "TE4", Serialnumber = "MY-515767"},
            };
            testEquipments.ForEach(a => db.TestEquipment.Add(a));
            db.SaveChanges();

            var testItems = new List<TestItem>()
            {
                new TestItem{Name = "驻波1"},
                new TestItem{Name = "驻波2"},
                new TestItem{Name = "衰减", Formular = "A+B = C"},
                new TestItem{Name = "时域阻抗"},
            };
            testItems.ForEach(a => db.TestItem.Add(a));
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
            var password = "123456";
            var userAdmin = new CamelDotNetUser();
            userAdmin.UserName = name;
            userAdmin.CamelDotNetRole = adminRole;
            UserManager.Create(userAdmin, password);

            name = "VNAT001";
            password = "123456";

            var tester001 = new CamelDotNetUser();
            tester001.UserName = name;
            tester001.CamelDotNetRole = vnaTesterRole;
            UserManager.Create(tester001, password);

            db.SaveChanges();

            base.Seed(db);
        }
    }
}