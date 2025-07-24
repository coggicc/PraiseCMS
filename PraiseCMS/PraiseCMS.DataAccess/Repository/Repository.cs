using PraiseCMS.DataAccess.DAL;
using PraiseCMS.DataAccess.Interfaces;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;

namespace PraiseCMS.DataAccess.Repository
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly ApplicationDbContext db = new ApplicationDbContext();
        private readonly DbSet<T> table = null;

        public Repository()
        {
            table = db.Set<T>();
        }

        public IEnumerable<T> GetAll()
        {
            return table.ToList();
        }

        public T GetById(object id)
        {
            return table.Find(id);
        }

        public void Insert(T obj)
        {
            table.Add(obj);
        }

        public void Update(T obj)
        {
            table.AddOrUpdate(obj);
        }

        public void Delete(object id)
        {
            T existing = table.Find(id);
            if (existing != null) table.Remove(existing);
        }

        public void Save()
        {
            db.SaveChanges();
        }
    }
}