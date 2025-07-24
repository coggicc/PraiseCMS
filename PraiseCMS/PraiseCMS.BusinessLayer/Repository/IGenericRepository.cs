using System.Collections.Generic;
using System.Linq;

namespace PraiseCMS.BusinessLayer.Repository
{
    public interface IGenericRepository
    {
        void Create<T>(T entity) where T : class;
        void Create<T>(IEnumerable<T> entities) where T : class;
        IQueryable<T> Read<T>() where T : class;
        void Update<T>(T entity) where T : class;
        void Delete<T>(T entity) where T : class;
        void Delete<T>(IEnumerable<T> entities) where T : class;
        int RunSQlCommand(string command);
        void SaveChanges();
    }
}