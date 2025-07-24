using PraiseCMS.BusinessLayer.Repository;
using PraiseCMS.DataAccess.DAL;
using PraiseCMS.DataAccess.Models;
using System.Collections.Generic;
using System.Linq;

namespace PraiseCMS.BusinessLayer
{
    public class PaymentMethodAccountOperations : GenericRepository
    {
        public PaymentMethodAccountOperations(ApplicationDbContext db, Work work)
         : base(db, work)
        {
        }

        public PaymentMethodAccount Get(string id)
        {
            return Read<PaymentMethodAccount>().FirstOrDefault(x => x.Id == id);
        }

        public PaymentMethodAccount GetByAccountGUID(string paymentMethods)
        {
            return Read<PaymentMethodAccount>().FirstOrDefault(x => x.AccountGUID.Equals(paymentMethods));
        }

        public string GetPrimaryAccountGUIDByDonorGUID(string donorGUID)
        {
            return Read<PaymentMethodAccount>().Where(x => x.DonorGUID == donorGUID && x.IsPrimary).Select(x => x.AccountGUID).FirstOrDefault();
        }

        public List<PaymentMethodAccount> GetAllByPaymentMethod(List<string> paymentMethods)
        {
            return Read<PaymentMethodAccount>().Where(x => paymentMethods.Contains(x.AccountGUID)).ToList();
        }

        public List<PaymentMethodAccount> GetAll(string donorGuid, bool includeInactive = false)
        {
            IQueryable<PaymentMethodAccount> query = Read<PaymentMethodAccount>().Where(x => x.DonorGUID == donorGuid);

            if (!includeInactive)
            {
                query = query.Where(x => x.IsActive);
            }

            return query.ToList();
        }

        #region CRUD
        public void Create(PaymentMethodAccount entity)
        {
            Create<PaymentMethodAccount>(entity);
            SaveChanges();
        }

        public void Update(PaymentMethodAccount entity)
        {
            Update<PaymentMethodAccount>(entity);
            SaveChanges();
        }

        public void Delete(string id)
        {
            var entity = Get(id);
            Delete<PaymentMethodAccount>(entity);
            SaveChanges();
        }

        public void Delete(PaymentMethodAccount entity)
        {
            Delete<PaymentMethodAccount>(entity);
            SaveChanges();
        }
        #endregion        
    }
}