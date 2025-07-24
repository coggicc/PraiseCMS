using PraiseCMS.BusinessLayer.Repository;
using PraiseCMS.DataAccess.DAL;
using PraiseCMS.DataAccess.Models;
using PraiseCMS.DataAccess.Services;
using PraiseCMS.DataAccess.Session;
using PraiseCMS.Shared.Models;
using PraiseCMS.Shared.Shared;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PraiseCMS.BusinessLayer
{
    public class SmallGroupOperations : GenericRepository
    {
        public SmallGroupOperations(ApplicationDbContext db, Work work)
            : base(db, work)
        {
        }

        public SmallGroup Get(string id)
        {
            return Read<SmallGroup>().FirstOrDefault(x => x.Id == id);
        }

        public List<SmallGroup> GetAll(string churchId, DateRange dateRange = null)
        {
            var query = Read<SmallGroup>().Where(x => x.ChurchId == churchId);

            if (dateRange != null)
            {
                query = query.Where(x => x.CreatedDate >= dateRange.StartDate && x.CreatedDate <= dateRange.EndDate);
            }

            return query.ToList();
        }

        public List<SmallGroup> GetAllByCampusId(string campusId)
        {
            return Read<SmallGroup>().Where(x => x.CampusId == campusId).ToList();
        }

        #region CRUD
        public Result<SmallGroup> Create(SmallGroup entity)
        {
            try
            {
                Create<SmallGroup>(entity);
                SaveChanges();
                return new Result<SmallGroup>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<SmallGroup>
                {
                    Data = entity,
                    Exception = ex,
                    Message = Constants.CreateExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<SmallGroup> Update(SmallGroup entity)
        {
            try
            {
                entity.ModifiedDate = DateTime.Now;
                entity.ModifiedBy = SessionVariables.CurrentUser.User.Id;

                Update<SmallGroup>(entity);
                SaveChanges();
                return new Result<SmallGroup>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<SmallGroup>
                {
                    Data = entity,
                    Exception = ex,
                    Message = Constants.UpdateExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<SmallGroup> Delete(SmallGroup entity)
        {
            try
            {
                Delete<SmallGroup>(entity);
                SaveChanges();
                return new Result<SmallGroup>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<SmallGroup>
                {
                    Data = entity,
                    Exception = ex,
                    Message = Constants.DeleteExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<SmallGroup> Delete(string id)
        {
            try
            {
                var entity = Get(id);
                Delete<SmallGroup>(entity);
                SaveChanges();
                return new Result<SmallGroup>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<SmallGroup>
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