using PraiseCMS.BusinessLayer.Repository;
using PraiseCMS.DataAccess.DAL;
using PraiseCMS.DataAccess.Models;
using PraiseCMS.DataAccess.Services;
using PraiseCMS.DataAccess.Session;
using PraiseCMS.Shared.Shared;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PraiseCMS.BusinessLayer
{
    public class IPBlacklistOperations : GenericRepository
    {
        public IPBlacklistOperations(ApplicationDbContext db, Work work)
            : base(db, work)
        {
        }

        public IPBlacklist Get(string id)
        {
            return Read<IPBlacklist>().FirstOrDefault(x => x.Id == id);
        }

        public List<IPBlacklist> GetAll()
        {
            return Read<IPBlacklist>().ToList();
        }

        #region CRUD
        public Result<IPBlacklist> Create(IPBlacklist entity)
        {
            try
            {
                Create<IPBlacklist>(entity);
                SaveChanges();
                return new Result<IPBlacklist>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<IPBlacklist>
                {
                    Data = entity,
                    Exception = ex,
                    Message = Constants.CreateExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<IPBlacklist> Update(IPBlacklist entity)
        {
            try
            {
                entity.ModifiedDate = DateTime.Now;
                entity.ModifiedBy = SessionVariables.CurrentUser.User.Id;
                Update<IPBlacklist>(entity);
                SaveChanges();
                return new Result<IPBlacklist>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<IPBlacklist>
                {
                    Data = entity,
                    Exception = ex,
                    Message = Constants.UpdateExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<IPBlacklist> Delete(IPBlacklist entity)
        {
            try
            {
                Delete<IPBlacklist>(entity);
                SaveChanges();
                return new Result<IPBlacklist>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<IPBlacklist>
                {
                    Data = entity,
                    Exception = ex,
                    Message = Constants.DeleteExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<IPBlacklist> Delete(string id)
        {
            try
            {
                var entity = Get(id);
                Delete<IPBlacklist>(entity);
                SaveChanges();
                return new Result<IPBlacklist>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<IPBlacklist>
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