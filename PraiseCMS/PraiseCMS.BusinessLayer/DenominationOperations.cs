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
    public class DenominationOperations : GenericRepository
    {
        public DenominationOperations(ApplicationDbContext db, Work work)
            : base(db, work)
        {
        }

        public Denomination Get(string id)
        {
            return Read<Denomination>().FirstOrDefault(x => x.Id == id);
        }

        public List<Denomination> GetAll()
        {
            return Read<Denomination>().OrderBy(x => x.Name).ToList();
        }

        #region CRUD
        public Result<Denomination> Create(Denomination entity)
        {
            try
            {
                Create<Denomination>(entity);
                SaveChanges();
                return new Result<Denomination>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<Denomination>
                {
                    Data = entity,
                    Exception = ex,
                    Message = Constants.CreateExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<Denomination> Update(Denomination entity)
        {
            try
            {
                Update<Denomination>(entity);
                SaveChanges();
                return new Result<Denomination>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<Denomination>
                {
                    Data = entity,
                    Exception = ex,
                    Message = Constants.UpdateExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<Denomination> Delete(Denomination entity)
        {
            try
            {
                Delete<Denomination>(entity);
                SaveChanges();
                return new Result<Denomination>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<Denomination>
                {
                    Data = entity,
                    Exception = ex,
                    Message = Constants.DeleteExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<Denomination> Delete(string id)
        {
            try
            {
                var entity = Get(id);
                Delete<Denomination>(entity);
                SaveChanges();
                return new Result<Denomination>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<Denomination>
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