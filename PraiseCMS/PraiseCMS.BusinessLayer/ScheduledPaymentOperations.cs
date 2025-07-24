using PraiseCMS.BusinessLayer.Repository;
using PraiseCMS.DataAccess.DAL;
using PraiseCMS.DataAccess.Models;
using PraiseCMS.DataAccess.Services;
using PraiseCMS.Shared.Methods;
using PraiseCMS.Shared.Shared;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PraiseCMS.BusinessLayer
{
    public class ScheduledPaymentOperations : GenericRepository
    {
        public ScheduledPaymentOperations(ApplicationDbContext db, Work work)
        : base(db, work)
        {
        }

        public ScheduledPayment Get(string id)
        {
            return Read<ScheduledPayment>().FirstOrDefault(x => x.Id == id);
        }

        public List<ScheduledPayment> GetAll(string churchId, string userId)
        {
            var scheduledPayments = GetList(userId, churchId);
            var paymentMethodAccounts = Work.PaymentMethodAccount.GetAllByPaymentMethod(scheduledPayments.Select(q => q.PaymentMethod).ToList());

            foreach (var sp in scheduledPayments)
            {
                var paymentMethodAccount = paymentMethodAccounts.FirstOrDefault(x => x.AccountGUID == sp.PaymentMethod);
                sp.PaymentMethod = paymentMethodAccount.IsNotNullOrEmpty() ? paymentMethodAccount.PaymentMethodPreview : string.Empty;
            }

            return scheduledPayments;
        }

        public List<ScheduledPayment> GetAllByFund(string fundId)
        {
            return Read<ScheduledPayment>().Where(x => x.FundId == fundId).ToList();
        }

        public List<ScheduledPayment> GetAllByMethod(string paymentMethodId)
        {
            return Read<ScheduledPayment>().Where(x => x.PaymentMethod == paymentMethodId).ToList();
        }

        public List<ScheduledPayment> GetAllUnprocessed(string userId = null, string churchId = null)
        {
            var query = Read<ScheduledPayment>().Where(x => !x.IsProcessed);

            if (!string.IsNullOrEmpty(userId))
            {
                query = query.Where(x => x.UserId == userId);
            }

            if (!string.IsNullOrEmpty(churchId))
            {
                query = query.Where(x => x.ChurchId == churchId);
            }

            return query.ToList();
        }

        public List<ScheduledPayment> GetList(string userId, string churchId)
        {
            return Read<ScheduledPayment>().Where(x => x.UserId == userId && x.ChurchId == churchId && x.IsActive).ToList();
        }

        public Result<ScheduledPayment> Deactivate(string id)
        {
            try
            {
                var entity = Get(id);

                // Set IsActive to 0 (false) to deactivate the scheduled payment
                entity.IsActive = false;

                // Update the entity in the database
                Update<ScheduledPayment>(entity);
                SaveChanges();

                return new Result<ScheduledPayment>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<ScheduledPayment>
                {
                    Exception = ex,
                    Message = Constants.UpdateExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        #region CRUD
        public Result<ScheduledPayment> Create(ScheduledPayment entity)
        {
            try
            {
                Create<ScheduledPayment>(entity);
                SaveChanges();
                return new Result<ScheduledPayment>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<ScheduledPayment>
                {
                    Data = entity,
                    Exception = ex,
                    Message = Constants.CreateExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }
        public Result<ScheduledPayment> Update(ScheduledPayment entity)
        {
            try
            {
                Update<ScheduledPayment>(entity);
                SaveChanges();
                return new Result<ScheduledPayment>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<ScheduledPayment>
                {
                    Data = entity,
                    Exception = ex,
                    Message = Constants.UpdateExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<ScheduledPayment> Delete(ScheduledPayment entity)
        {
            try
            {
                Delete<ScheduledPayment>(entity);
                SaveChanges();
                return new Result<ScheduledPayment>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<ScheduledPayment>
                {
                    Data = entity,
                    Exception = ex,
                    Message = Constants.DeleteExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<ScheduledPayment> Delete(string id)
        {
            try
            {
                var entity = Get(id);
                Delete<ScheduledPayment>(entity);
                SaveChanges();
                return new Result<ScheduledPayment>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<ScheduledPayment>
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