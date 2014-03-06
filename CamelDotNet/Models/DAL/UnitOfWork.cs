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