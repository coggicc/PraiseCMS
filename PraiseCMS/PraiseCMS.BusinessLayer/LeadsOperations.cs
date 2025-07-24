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
    public class LeadsOperations : GenericRepository
    {
        public LeadsOperations(ApplicationDbContext db, Work work) : base(db, work)
        {
        }

        public Lead Get(string id)
        {
            return Read<Lead>().FirstOrDefault(x => x.Id == id);
        }

        public IEnumerable<Lead> GetAll()
        {
            return Read<Lead>().ToList().OrderByDescending(q => q.CreatedDate);
        }

        public IEnumerable<Lead> GetAll(List<string> ids)
        {
            return Read<Lead>().Where(x => ids.Contains(x.Id)).ToList();
        }

        #region CRUD
        public Result<Lead> Create(Lead entity)
        {
            try
            {
                Create<Lead>(entity);
                SaveChanges();
                return new Result<Lead>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<Lead>
                {
                    Data = entity,
                    Exception = ex,
                    Message = Constants.CreateExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<Lead> Update(Lead entity)
        {
            try
            {
                entity.ModifiedDate = DateTime.Now;
                entity.ModifiedBy = SessionVariables.CurrentUser.User.Id;

                Update<Lead>(entity);
                SaveChanges();
                return new Result<Lead>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<Lead>
                {
                    Data = entity,
                    Exception = ex,
                    Message = Constants.UpdateExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<Lead> Delete(Lead entity)
        {
            try
            {
                Delete<Lead>(entity);
                SaveChanges();
                return new Result<Lead>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<Lead>
                {
                    Data = entity,
                    Exception = ex,
                    Message = Constants.DeleteExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<Lead> Delete(string id)
        {
            try
            {
                var entity = Get(id);
                Delete<Lead>(entity);
                SaveChanges();
                return new Result<Lead>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<Lead>
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