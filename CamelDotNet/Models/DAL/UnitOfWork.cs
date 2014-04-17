using CamelDotNet.Models.Base;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace CamelDotNet.Models.DAL
{
    public class UnitOfWork : IDisposable
    {
        public CamelDotNetDBContext context = new CamelDotNetDBContext();
        private GenericRepository<TestItem> testItemRepository;
        private GenericRepository<TestEquipment> testEquipmentRepository;
        private GenericRepository<Process> processRepository;
        private GenericRepository<TestStation> testStationRepository;
        private GenericRepository<Client> clientRepository;
        private GenericRepository<Permission> permissionRepository;
        private GenericRepository<CamelDotNetRole> camelDotNetRoleRepository;
        private GenericRepository<CamelDotNetUser> camelDotNetUserRepository;
        private GenericRepository<ProductType> productTypeRepository;
        private GenericRepository<TestItemCategory> testItemCategoryRepository;
        private GenericRepository<TestConfig> testConfigRepository;
        private GenericRepository<TestItemConfig> testItemConfigRepository;
        private GenericRepository<PerConfig> perConfigRepository;
        private GenericRepository<SerialNumber> serialNumberRepository;
        private GenericRepository<VnaRecord> vnaRecordRepository;
        private GenericRepository<VnaTestItemRecord> vnaTestItemRecordRepository;
        private GenericRepository<VnaTestItemPerRecord> vnaTestItemPerRecordRepository;
        private GenericRepository<Unit> unitRepository;
        private GenericRepository<Department> departmentRepository;
        private GenericRepository<QualityPassRecord> qualityPassRecordRepository;
        public GenericRepository<TestItem> TestItemRepository 
        {
            get 
            {
                if(this.testItemRepository == null)
                {
                    this.testItemRepository = new GenericRepository<TestItem>(context);                
                }
                return testItemRepository;
            }
        }
        public GenericRepository<TestEquipment> TestEquipmentRepository
        {
            get
            {
                if (this.testEquipmentRepository == null)
                {
                    this.testEquipmentRepository = new GenericRepository<TestEquipment>(context);
                }
                return testEquipmentRepository;
            }
        }
        public GenericRepository<TestStation> TestStationRepository
        {
            get
            {
                if (this.testStationRepository == null)
                {
                    this.testStationRepository = new GenericRepository<TestStation>(context);
                }
                return testStationRepository;
            }
        }
        public GenericRepository<Process> ProcessRepository
        {
            get
            {
                if (this.processRepository == null)
                {
                    this.processRepository = new GenericRepository<Process>(context);
                }
                return processRepository;
            }
        }

        public GenericRepository<Client> ClientRepository
        {
            get
            {
                if (this.clientRepository == null)
                {
                    this.clientRepository = new GenericRepository<Client>(context);
                }
                return clientRepository;
            }
        }

        public GenericRepository<Permission> PermissionRepository
        {
            get
            {
                if (this.permissionRepository == null)
                {
                    this.permissionRepository = new GenericRepository<Permission>(context);
                }
                return permissionRepository;
            }
        }

        public GenericRepository<CamelDotNetRole> CamelDotNetRoleRepository
        {
            get
            {
                if (this.camelDotNetRoleRepository == null)
                {
                    this.camelDotNetRoleRepository = new GenericRepository<CamelDotNetRole>(context);
                }
                return camelDotNetRoleRepository;
            }
        }

        public GenericRepository<CamelDotNetUser> CamelDotNetUserRepository
        {
            get
            {
                if (this.camelDotNetUserRepository == null)
                {
                    this.camelDotNetUserRepository = new GenericRepository<CamelDotNetUser>(context);
                }
                return camelDotNetUserRepository;
            }
        }

        public GenericRepository<ProductType> ProductTypeRepository
        {
            get
            {
                if (this.productTypeRepository == null)
                {
                    this.productTypeRepository = new GenericRepository<ProductType>(context);
                }
                return productTypeRepository;
            }
        }

        public GenericRepository<TestItemCategory> TestItemCategoryRepository
        {
            get
            {
                if (this.testItemCategoryRepository == null)
                {
                    this.testItemCategoryRepository = new GenericRepository<TestItemCategory>(context);
                }
                return testItemCategoryRepository;
            }
        }

        public GenericRepository<TestConfig> TestConfigRepository
        {
            get
            {
                if (this.testConfigRepository == null)
                {
                    this.testConfigRepository = new GenericRepository<TestConfig>(context);
                }
                return testConfigRepository;
            }
        }

        public GenericRepository<TestItemConfig> TestItemConfigRepository
        {
            get
            {
                if (this.testItemConfigRepository == null)
                {
                    this.testItemConfigRepository = new GenericRepository<TestItemConfig>(context);
                }
                return testItemConfigRepository;
            }
        }

        public GenericRepository<PerConfig> PerConfigRepository
        {
            get
            {
                if (this.perConfigRepository == null)
                {
                    this.perConfigRepository = new GenericRepository<PerConfig>(context);
                }
                return perConfigRepository;
            }
        }

        public GenericRepository<SerialNumber> SerialNumberRepository
        {
            get
            {
                if (this.serialNumberRepository == null)
                {
                    this.serialNumberRepository = new GenericRepository<SerialNumber>(context);
                }
                return serialNumberRepository;
            }
        }

        public GenericRepository<VnaRecord> VnaRecordRepository
        {
            get
            {
                if (this.vnaRecordRepository == null)
                {
                    this.vnaRecordRepository = new GenericRepository<VnaRecord>(context);
                }
                return vnaRecordRepository;
            }
        }

        public GenericRepository<VnaTestItemRecord> VnaTestItemRecordRepository
        {
            get
            {
                if (this.vnaTestItemRecordRepository == null)
                {
                    this.vnaTestItemRecordRepository = new GenericRepository<VnaTestItemRecord>(context);
                }
                return vnaTestItemRecordRepository;
            }
        }

        public GenericRepository<VnaTestItemPerRecord> VnaTestItemPerRecordRepository
        {
            get
            {
                if (this.vnaTestItemPerRecordRepository == null)
                {
                    this.vnaTestItemPerRecordRepository = new GenericRepository<VnaTestItemPerRecord>(context);
                }
                return vnaTestItemPerRecordRepository;
            }
        }

        public GenericRepository<Unit> UnitRepository
        {
            get
            {
                if (this.unitRepository == null)
                {
                    this.unitRepository = new GenericRepository<Unit>(context);
                }
                return unitRepository;
            }
        }

        public GenericRepository<Department> DepartmentRepository
        {
            get
            {
                if (this.departmentRepository == null)
                {
                    this.departmentRepository = new GenericRepository<Department>(context);
                }
                return departmentRepository;
            }
        }

        public GenericRepository<QualityPassRecord> QualityPassRecordRepository
        {
            get
            {
                if (this.qualityPassRecordRepository == null)
                {
                    this.qualityPassRecordRepository = new GenericRepository<QualityPassRecord>(context);
                }
                return qualityPassRecordRepository;
            }
        }

        public void CamelSave() 
        {
            foreach(var deletedEntity in context.ChangeTracker.Entries<BaseModel>())
            {
                if(deletedEntity.State == EntityState.Deleted)
                {
                    deletedEntity.State = EntityState.Unchanged;
                    deletedEntity.Entity.IsDeleted = true;
                }
            }
            context.SaveChanges();
        }

        public void CamelUserSave()
        {
            foreach (var deletedEntity in context.ChangeTracker.Entries<CamelDotNetUser>())
            {
                if (deletedEntity.State == EntityState.Deleted)
                {
                    deletedEntity.State = EntityState.Unchanged;
                    deletedEntity.Entity.IsDeleted = true;
                }
            }
            context.SaveChanges();
        }

        public void CamelTestConfigSave()
        {
            foreach (var deletedEntity in context.ChangeTracker.Entries<TestConfig>())
            {
                if (deletedEntity.State == EntityState.Deleted)
                {
                    deletedEntity.State = EntityState.Unchanged;
                    deletedEntity.Entity.IsDeleted = true;
                }
            }
            context.SaveChanges();
        }

        private bool disposed = false;
        protected virtual void Dispose(bool disposing) 
        {
            if(!this.disposed)
            {
                if(disposing)
                {
                    context.Dispose();
                }
                this.disposed = true;
            }
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}