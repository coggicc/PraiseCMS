using PraiseCMS.BusinessLayer.Repository;
using PraiseCMS.DataAccess.DAL;
using PraiseCMS.DataAccess.Models;
using System.Linq;

namespace PraiseCMS.BusinessLayer
{
    public class UserMerchantAccountOperations : GenericRepository
    {
        public UserMerchantAccountOperations(ApplicationDbContext db, Work work)
         : base(db, work)
        {
        }

        public UserMerchantAccount Get(string id)
        {
            return Read<UserMerchantAccount>().FirstOrDefault(x => x.Id == id);
        }

        public UserMerchantAccount GetByUserId(string userId, string merchant = "")
        {
            IQueryable<UserMerchantAccount> query = Read<UserMerchantAccount>()
                                                      .Where(x => x.UserId == userId && x.IsActive);

            if (!string.IsNullOrEmpty(merchant))
            {
                // Apply additional filter based on the merchant when it's not null or empty
                query = query.Where(x => x.Merchant == merchant);
            }

            return query.FirstOrDefault();
        }

        #region CRUD
        public void Create(UserMerchantAccount entity)
        {
            Create<UserMerchantAccount>(entity);
            SaveChanges();
        }

        public void Update(UserMerchantAccount entity)
        {
            Update<UserMerchantAccount>(entity);
            SaveChanges();
        }
        public void Delete(string id)
        {
            var entity = Get(id);
            Delete<UserMerchantAccount>(entity);
            SaveChanges();
        }

        public void Delete(UserMerchantAccount entity)
        {
            Delete<UserMerchantAccount>(entity);
            SaveChanges();
        }
        #endregion
    }
}