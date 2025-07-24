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
    public class IPWhitelistOperations : GenericRepository
    {
        public IPWhitelistOperations(ApplicationDbContext db, Work work)
            : base(db, work)
        {
        }

        public IPWhitelist Get(string id)
        {
            return Read<IPWhitelist>().FirstOrDefault(x => x.Id == id);
        }

        public List<IPWhitelist> GetAll()
        {
            return Read<IPWhitelist>().OrderBy(x => x.Name).ToList();
        }

        #region CRUD
        public Result<IPWhitelist> Create(IPWhitelist entity)
        {
            try
            {
                Create<IPWhitelist>(entity);
                SaveChanges();
                return new Result<IPWhitelist>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<IPWhitelist>
                {
                    Data = entity,
                    Exception = ex,
                    Message = Constants.CreateExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<IPWhitelist> Update(IPWhitelist entity)
        {
            try
            {
                Update<IPWhitelist>(entity);
                SaveChanges();
                return new Result<IPWhitelist>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<IPWhitelist>
                {
                    Data = entity,
                    Exception = ex,
                    Message = Constants.UpdateExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<IPWhitelist> Delete(IPWhitelist entity)
        {
            try
            {
                Delete<IPWhitelist>(entity);
                SaveChanges();
                return new Result<IPWhitelist>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<IPWhitelist>
                {
                    Data = entity,
                    Exception = ex,
                    Message = Constants.DeleteExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<IPWhitelist> Delete(string id)
        {
            try
            {
                var entity = Get(id);
                Delete<IPWhitelist>(entity);
                SaveChanges();
                return new Result<IPWhitelist>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<IPWhitelist>
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