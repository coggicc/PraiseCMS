using PraiseCMS.BusinessLayer.Repository;
using PraiseCMS.DataAccess.DAL;
using PraiseCMS.DataAccess.Models;
using System.Collections.Generic;
using System.Linq;

namespace PraiseCMS.BusinessLayer
{
    public class SupportOperations : GenericRepository
    {
        public SupportOperations(ApplicationDbContext db, Work work)
            : base(db, work)
        {
        }

        public List<Email> GetAll(string userId)
        {
            return Read<Email>().Where(x => x.IsSupportEmail && x.CreatedBy.Equals(userId)).OrderByDescending(q => q.CreatedDate).ToList();
        }
    }
}