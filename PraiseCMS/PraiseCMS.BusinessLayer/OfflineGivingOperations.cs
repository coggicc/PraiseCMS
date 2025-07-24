using PraiseCMS.BusinessLayer.Repository;
using PraiseCMS.DataAccess.DAL;
using PraiseCMS.DataAccess.Models;
using PraiseCMS.DataAccess.Services;
using PraiseCMS.DataAccess.Session;
using PraiseCMS.Shared.Models;
using PraiseCMS.Shared.Shared;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace PraiseCMS.BusinessLayer
{
    public class OfflineGivingOperations : GenericRepository
    {
        public OfflineGivingOperations(ApplicationDbContext db, Work work)
            : base(db, work)
        {
        }

        public OfflineGiving Get(string id)
        {
            return Read<OfflineGiving>().FirstOrDefault(x => x.Id == id);
        }

        public List<OfflineGiving> GetAll()
        {
            return Read<OfflineGiving>().ToList();
        }

        public List<OfflineGiving> GetAll(string churchId, string fundId = null, DateRange dateRange = null)
        {
            var query = Read<OfflineGiving>().Where(x => x.ChurchId == churchId);

            if (!string.IsNullOrEmpty(fundId))
            {
                query = query.Where(x => x.FundId == fundId);
            }

            if (dateRange != null)
            {
                query = query.Where(x => (x.DateReceived != null
                        && DbFunctions.TruncateTime(x.DateReceived) >= dateRange.StartDate
                        && DbFunctions.TruncateTime(x.DateReceived) <= dateRange.EndDate)
                    || (x.DateReceived == null
                        && DbFunctions.TruncateTime(x.CreatedDate) >= dateRange.StartDate
                        && DbFunctions.TruncateTime(x.CreatedDate) <= dateRange.EndDate));
            }

            return query.ToList();
        }

        public List<OfflineGiving> GetAllByCampusId(string campusId, DateRange dateRange = null)
        {
            var query = Read<OfflineGiving>().Where(x => x.CampusId == campusId);

            if (dateRange != null)
            {
                query = query.Where(x => (x.DateReceived != null
                        && DbFunctions.TruncateTime(x.DateReceived) >= dateRange.StartDate
                        && DbFunctions.TruncateTime(x.DateReceived) <= dateRange.EndDate)
                    || (x.DateReceived == null
                        && DbFunctions.TruncateTime(x.CreatedDate) >= dateRange.StartDate
                        && DbFunctions.TruncateTime(x.CreatedDate) <= dateRange.EndDate));
            }

            return query.ToList();
        }

        public List<OfflineGiving> GetAllByPersonId(string churchId, string personId)
        {
            return Read<OfflineGiving>().Where(x => x.PersonId.Equals(personId) && x.ChurchId == churchId).OrderByDescending(x => x.DateReceived).ToList();
        }

        public List<OfflineGiving> GetAllByUserId(string churchId, string userId, int count)
        {
            var qty = count > 0 ? count : 10;
            var person = Work.Person.GetByUserId(userId);
            var payment = Read<OfflineGiving>().Where(x => x.PersonId == person.Id && x.ChurchId == churchId).OrderByDescending(x => x.DateReceived).ToList();

            return payment.Count >= qty ? payment.Take(qty).ToList() : payment;
        }

        public List<OfflineGiving> GetAllWithDonors(string churchId, DateRange dateRange = null)
        {
            var query = Read<OfflineGiving>().Where(x => x.ChurchId == churchId && x.PersonId != null);

            if (dateRange != null)
            {
                query = query.Where(x => (x.DateReceived != null
                        && DbFunctions.TruncateTime(x.DateReceived) >= dateRange.StartDate
                        && DbFunctions.TruncateTime(x.DateReceived) <= dateRange.EndDate)
                    || (x.DateReceived == null
                        && DbFunctions.TruncateTime(x.CreatedDate) >= dateRange.StartDate
                        && DbFunctions.TruncateTime(x.CreatedDate) <= dateRange.EndDate));
            }

            return query.ToList();
        }

        public async Task<List<OfflineGiving>> GetAllWithDonorsAsync(string churchId, DateRange dateRange = null)
        {
            var query = Read<OfflineGiving>().Where(x => x.ChurchId == churchId && x.PersonId != null);

            if (dateRange != null)
            {
                query = query.Where(x => (x.DateReceived != null
                        && DbFunctions.TruncateTime(x.DateReceived) >= dateRange.StartDate
                        && DbFunctions.TruncateTime(x.DateReceived) <= dateRange.EndDate)
                    || (x.DateReceived == null
                        && DbFunctions.TruncateTime(x.CreatedDate) >= dateRange.StartDate
                        && DbFunctions.TruncateTime(x.CreatedDate) <= dateRange.EndDate));
            }

            return await query.ToListAsync();
        }

        #region CRUD
        public Result<OfflineGiving> Create(OfflineGiving entity)
        {
            try
            {
                Create<OfflineGiving>(entity);
                SaveChanges();
                return new Result<OfflineGiving>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<OfflineGiving>
                {
                    Data = entity,
                    Exception = ex,
                    Message = Constants.CreateExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<OfflineGiving> Create(IEnumerable<OfflineGiving> entity)
        {
            try
            {
                Create<OfflineGiving>(entity);
                SaveChanges();
                return new Result<OfflineGiving> { ResultType = ResultType.Success };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<OfflineGiving>
                {
                    Exception = ex,
                    Message = Constants.CreateExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<OfflineGiving> Update(OfflineGiving entity)
        {
            try
            {
                entity.ModifiedDate = DateTime.Now;
                entity.ModifiedBy = SessionVariables.CurrentUser.User.Id;

                Update<OfflineGiving>(entity);
                SaveChanges();
                return new Result<OfflineGiving>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<OfflineGiving>
                {
                    Data = entity,
                    Exception = ex,
                    Message = Constants.UpdateExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<OfflineGiving> Delete(OfflineGiving entity)
        {
            try
            {
                Delete<OfflineGiving>(entity);
                SaveChanges();
                return new Result<OfflineGiving>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<OfflineGiving>
                {
                    Data = entity,
                    Exception = ex,
                    Message = Constants.DeleteExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<OfflineGiving> Delete(string id)
        {
            try
            {
                var entity = Get(id);
                Delete<OfflineGiving>(entity);
                SaveChanges();
                return new Result<OfflineGiving>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<OfflineGiving>
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