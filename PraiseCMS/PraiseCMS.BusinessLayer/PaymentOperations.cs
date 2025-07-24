using PraiseCMS.BusinessLayer.Repository;
using PraiseCMS.DataAccess.DAL;
using PraiseCMS.DataAccess.Models;
using PraiseCMS.DataAccess.Services;
using PraiseCMS.DataAccess.Shared;
using PraiseCMS.Shared.Methods;
using PraiseCMS.Shared.Models;
using PraiseCMS.Shared.Shared;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace PraiseCMS.BusinessLayer
{
    public class PaymentOperations : GenericRepository
    {
        public PaymentOperations(ApplicationDbContext db, Work work)
            : base(db, work)
        {
        }

        public Payment Get(string id)
        {
            return Read<Payment>().FirstOrDefault(x => x.Id == id);
        }

        public Payment GetByTransactionId(string transactionId)
        {
            return Read<Payment>().FirstOrDefault(x => x.TransactionId == transactionId);
        }

        public List<Payment> GetAll()
        {
            var payments = Read<Payment>().ToList();
            return GetSuccessfulPayments(payments);
        }

        public List<Payment> GetAll(string churchId, string fundId = null, DateRange dateRange = null)
        {
            // Start with the base query filtering by churchId
            IQueryable<Payment> query = Read<Payment>().Where(x => x.ChurchId == churchId);

            // If a fundId is provided, filter by fundId as well
            if (!string.IsNullOrEmpty(fundId))
            {
                query = query.Where(x => x.FundId == fundId);
            }

            // If a dateRange is provided, filter by the CreatedDate within the range
            if (dateRange != null)
            {
                query = query.Where(x => DbFunctions.TruncateTime(x.CreatedDate) >= dateRange.StartDate.Date
                                        && DbFunctions.TruncateTime(x.CreatedDate) <= dateRange.EndDate.Date);
            }

            // Retrieve and filter successful payments
            return GetSuccessfulPayments(query.ToList());
        }

        public List<Payment> GetAllByCampusId(string campusId, DateRange dateRange = null)
        {
            // Start with the base query filtering by campusId
            IQueryable<Payment> query = Read<Payment>().Where(x => x.CampusId == campusId);

            // If a dateRange is provided, filter by the CreatedDate within the range
            if (dateRange != null)
            {
                query = query.Where(x => DbFunctions.TruncateTime(x.CreatedDate) >= dateRange.StartDate.Date
                                        && DbFunctions.TruncateTime(x.CreatedDate) <= dateRange.EndDate.Date);
            }

            // Retrieve and filter successful payments
            return GetSuccessfulPayments(query.ToList());
        }

        public List<Payment> GetAllByUserId(string churchId, string userId, DateRange dateRange = null, int? count = null)
        {
            IQueryable<Payment> query = Read<Payment>()
                .Where(x => x.UserId == userId && x.ChurchId == churchId);

            if (dateRange != null)
            {
                query = query.Where(x => x.CreatedDate.Date >= dateRange.StartDate.Date && x.CreatedDate.Date <= dateRange.EndDate.Date);
            }

            var payments = GetSuccessfulPayments(query.ToList());

            return count.HasValue
                ? payments.OrderByDescending(x => x.CreatedDate).Take(count.Value).ToList()
                : payments.OrderByDescending(x => x.CreatedDate).ToList();
        }

        public List<Payment> GetAllForYear(string churchId, int year, string userId = null)
        {
            IQueryable<Payment> query = Read<Payment>()
                .Where(x => x.ChurchId == churchId && x.CreatedDate.Year == year);

            if (!string.IsNullOrEmpty(userId))
            {
                query = query.Where(x => x.UserId == userId);
            }

            return GetSuccessfulPayments(query.ToList());
        }

        public async Task<List<Payment>> GetAllForYearAsync(string churchId, int year)
        {
            var payments = await Read<Payment>()
                .Where(x => x.ChurchId == churchId && x.CreatedDate.Year == year)
                .ToListAsync();

            return GetSuccessfulPayments(payments);
        }

        public List<int> GetFilteredPaymentYears(string churchId, string userId)
        {
            var currentYear = DateTime.Now.Year;

            return Read<Payment>()
                .Where(payment => payment.UserId == userId && payment.ChurchId == churchId && payment.CreatedDate.Year != currentYear)
                .Select(payment => payment.CreatedDate.Year)
                .Distinct()
                .OrderByDescending(year => year)
                .ToList();
        }

        public List<PersonDigitalGiving> GetPersonDigitalGiving(List<Payment> payments)
        {
            var personDigitalGivingList = new List<PersonDigitalGiving>();
            var people = Work.Person.GetAllByUserIds(payments.Select(x => x.UserId).ToList());

            foreach (Payment payment in payments)
            {
                PersonDigitalGiving pDG = new PersonDigitalGiving
                {
                    Payment = payment,
                    Person = people.FirstOrDefault(x => x.Id == payment.UserId)
                };
                personDigitalGivingList.Add(pDG);
            }

            return personDigitalGivingList;
        }

        public PaymentMethodViewModel GetPaymentMethods(ApplicationUser currentUser, List<UserMerchantAccount> userMerchantAccounts, string churchDonorGuid, bool forChurch = false)
        {
            var viewModel = new PaymentMethodViewModel
            {
                User = currentUser,
                DonorGUID = forChurch ? churchDonorGuid : GetUserDonorGuid(currentUser, userMerchantAccounts)
            };

            if (viewModel.DonorGUID.IsNotNullOrEmpty())
            {
                var accounts = forChurch ? Work.Giving.GetChurchPaymentMethodsList(viewModel.DonorGUID) : Work.Giving.GetPaymentMethodsList(viewModel.DonorGUID);

                viewModel.PaymentMethods = new PaymentMethods
                {
                    BankAccounts = accounts.BankAccounts,
                    CreditCards = accounts.CreditCards
                };

                SetPrimaryAutomatically(viewModel.DonorGUID, forChurch);
                viewModel.PrimaryAccountGUID = GetPrimaryAccount(viewModel.DonorGUID);
            }

            return viewModel;
        }

        public List<SelectListItems> GetPaymentMethodsDropdownList(string donorGuid)
        {
            return Work.PaymentMethodAccount.GetAll(donorGuid)
                .Where(q => q.AccountType != DigitalPaymentMethods.Card || Utilities.CardExpirationCalculateInDays(q.ExpMonth, q.ExpYear) > 1)
                .Select(s => new SelectListItems
                {
                    Text = s.PaymentMethodPreview,
                    Value = s.AccountGUID,
                    IsPrimary = s.IsPrimary,
                    Selected = s.IsPrimary
                })
                .OrderBy(q => q.Text)
                .ToList();
        }

        public List<Payment> GetSuccessfulPayments(List<Payment> payments)
        {
            return payments
                .Where(x => !string.IsNullOrEmpty(x.PaymentStatus) && x.PaymentStatus.Equals(PaymentStatus.Success)
                    && !string.IsNullOrEmpty(x.TransactionType) && x.TransactionType.Equals(TransactionType.Payment))
                .ToList();
        }

        private string GetUserDonorGuid(ApplicationUser currentUser, List<UserMerchantAccount> userMerchantAccounts)
        {
            if (userMerchantAccounts.Any(x => x.Merchant.Equals(MerchantProviders.Nuvei)))
            {
                var userMerchantAccount = userMerchantAccounts.Find(x => x.Merchant.Equals(MerchantProviders.Nuvei));
                return userMerchantAccount.DonorGUID;
            }
            else
            {
                var userMerchantAccount = Work.UserMerchantAccount.GetByUserId(currentUser.Id);
                return userMerchantAccount?.DonorGUID ?? string.Empty;
            }
        }

        internal string GetPrimaryAccount(string donorGUID)
        {
            return Read<PaymentMethodAccount>().Where(q => q.DonorGUID == donorGUID && q.IsPrimary).Select(q => q.AccountGUID).FirstOrDefault();
        }

        public void SetPrimaryAutomatically(string donorGUID, bool forChurch = false)
        {
            var methods = Work.Payment.GetPaymentMethodsDropdownList(donorGUID);

            var singleMethod = methods.SingleOrDefault(q => q.IsPrimary);

            if (singleMethod == null && methods.Count == 1)
            {
                var method = methods[0];
                Work.Giving.MakePrimary(method.Value, donorGUID, forChurch || true);
            }
        }

        #region CRUD
        public Result<Payment> Create(Payment entity)
        {
            try
            {
                if (string.IsNullOrEmpty(entity.TransactionId))
                {
                    entity.TransactionId = Utilities.GenerateUniqueId();
                }
                Create<Payment>(entity);
                SaveChanges();
                return new Result<Payment>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<Payment>
                {
                    Data = entity,
                    Exception = ex,
                    Message = Constants.CreateExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<Payment> CreateOrUpdate(Payment entity)
        {
            try
            {
                if (entity.Id.IsNotNullOrEmpty())
                {
                    Update(entity);
                }
                else
                {
                    Create<Payment>(entity);
                }

                SaveChanges();
                return new Result<Payment>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<Payment>
                {
                    Data = entity,
                    Exception = ex,
                    Message = Constants.CreateExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<Payment> Update(Payment entity)
        {
            try
            {
                Update<Payment>(entity);
                SaveChanges();
                return new Result<Payment>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<Payment>
                {
                    Data = entity,
                    Exception = ex,
                    Message = Constants.UpdateExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<Payment> Delete(Payment entity)
        {
            try
            {
                Delete<Payment>(entity);
                SaveChanges();
                return new Result<Payment>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<Payment>
                {
                    Data = entity,
                    Exception = ex,
                    Message = Constants.DeleteExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<Payment> Delete(string id)
        {
            try
            {
                var entity = Get(id);
                Delete<Payment>(entity);
                SaveChanges();
                return new Result<Payment>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<Payment>
                {
                    Exception = ex,
                    Message = Constants.DeleteExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }
        #endregion
    }
}