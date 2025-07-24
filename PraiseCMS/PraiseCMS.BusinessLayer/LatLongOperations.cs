using PraiseCMS.BusinessLayer.Repository;
using PraiseCMS.DataAccess.DAL;
using PraiseCMS.DataAccess.Models;
using PraiseCMS.DataAccess.Services;
using PraiseCMS.Shared.Shared;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PraiseCMS.BusinessLayer
{
    public class LatLongOperations : GenericRepository
    {
        public LatLongOperations(ApplicationDbContext db, Work work)
            : base(db, work)
        {
        }

        public LatLong Get(string id)
        {
            return Read<LatLong>().FirstOrDefault(x => x.Id == id);
        }

        public List<LatLong> GetAll()
        {
            return Read<LatLong>().ToList();
        }

        #region CRUD
        public Result<LatLong> Create(LatLong entity)
        {
            try
            {
                Create<LatLong>(entity);
                SaveChanges();
                return new Result<LatLong>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<LatLong>
                {
                    Data = entity,
                    Exception = ex,
                    Message = Constants.CreateExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<LatLong> Delete(LatLong entity)
        {
            try
            {
                Delete<LatLong>(entity);
                SaveChanges();
                return new Result<LatLong>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<LatLong>
                {
                    Data = entity,
                    Exception = ex,
                    Message = Constants.DeleteExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<LatLong> Delete(string id)
        {
            try
            {
                var entity = Get(id);
                Delete<LatLong>(entity);
                SaveChanges();
                return new Result<LatLong>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<LatLong>
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