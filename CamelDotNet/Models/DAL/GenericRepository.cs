using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace CamelDotNet.Models.DAL
{
    public class GenericRepository<TEntity> where TEntity : class
    {
        // Only accessible within the same assembly
        internal CamelDotNetDBContext context;
        internal DbSet<TEntity> dbSet;

        public GenericRepository(CamelDotNetDBContext context) 
        {
            this.context = context;
            this.dbSet = context.Set<TEntity>();
        }

        public virtual IQueryable<TEntity> Get(bool noTrack) 
        {
            if(noTrack)
            {
                return dbSet.AsNoTracking();
            }
            else
            {
                return dbSet;
            }
        }

        public virtual TEntity GetByID(object id) 
        {
            return dbSet.Find(id);
        }

        public virtual void Insert(TEntity entity) 
        {
            dbSet.Add(entity);
        }

        public virtual void Delete(object id) 
        {
            TEntity entity = dbSet.Find(id);
            Delete(entity);
        }

        public virtual void Delete(TEntity entity) 
        {
            if(context.Entry(entity).State == EntityState.Detached)
            {
                dbSet.Attach(entity);
            }
            dbSet.Remove(entity);
        }

        public virtual void Update(TEntity entity) 
        {
            dbSet.Attach(entity);
            context.Entry(entity).State = EntityState.Modified;
        }
    }
}