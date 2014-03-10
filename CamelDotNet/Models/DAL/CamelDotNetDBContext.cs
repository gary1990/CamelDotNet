﻿using Microsoft.AspNet.Identity;
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
            };
            testStations.ForEach(a => db.TestStation.Add(a));
            db.SaveChanges();

            var testEquipments = new List<TestEquipment>()
            {
                new TestEquipment{Name = "TE1", Serialnumber = "MY-123456"},
                new TestEquipment{Name = "TE2", Serialnumber = "MY-908097"},
                new TestEquipment{Name = "TE3", Serialnumber = "MY-668678"},
                new TestEquipment{Name = "TE4", Serialnumber = "MY-515767"},
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
            };
            productTypes.ForEach(a => db.ProductType.Add(a));
            db.SaveChanges();

            var clients = new List<Client>()
            {
                new Client{Name = "C1"},
                new Client{Name = "C2"},
                new Client{Name = "C3", IsDeleted = true},
                new Client{Name = "C11"},
                new Client{Name = "Client1"},
            };
            clients.ForEach(a => db.Client.Add(a));
            db.SaveChanges();

            var testConfigs = new List<TestConfig>() 
            {
                new TestConfig{ClientId = 1, ProductTypeId = 1},
                new TestConfig{ClientId = 1, ProductTypeId = 2},
                new TestConfig{ClientId = 1, ProductTypeId = 3},
                new TestConfig{ClientId = 2, ProductTypeId = 1},
                new TestConfig{ClientId = 2, ProductTypeId = 2},
                new TestConfig{ClientId = 2, ProductTypeId = 3},
                new TestConfig{ClientId = 3, ProductTypeId = 1},
                new TestConfig{ClientId = 3, ProductTypeId = 2},
                new TestConfig{ClientId = 3, ProductTypeId = 3},
            };
            testConfigs.ForEach(a => db.TestConfig.Add(a));
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
                new TestItem{Name = "驻波1", TestItemCategoryId = 1},
                new TestItem{Name = "驻波2", TestItemCategoryId = 1},
                new TestItem{Name = "衰减", Formular = "A+B = C", TestItemCategoryId = 1},
                new TestItem{Name = "时域阻抗", TestItemCategoryId = 1 },
                new TestItem{Name = "外观不合格", TestItemCategoryId = 2 },
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
            var jobNumber = "GC0001";
            var password = "123456";
            var userAdmin = new CamelDotNetUser();
            userAdmin.UserName = name;
            userAdmin.JobNumber = jobNumber;
            userAdmin.CamelDotNetRole = adminRole;
            UserManager.Create(userAdmin, password);

            name = "VNAT001";
            jobNumber = "GC1000";
            password = "123456";

            var tester001 = new CamelDotNetUser();
            tester001.UserName = name;
            tester001.JobNumber = jobNumber;
            tester001.CamelDotNetRole = vnaTesterRole;
            UserManager.Create(tester001, password);

            db.SaveChanges();

            base.Seed(db);
        }
    }
}