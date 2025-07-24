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
    public class ServiceAreaOperations : GenericRepository
    {
        public ServiceAreaOperations(ApplicationDbContext db, Work work)
            : base(db, work)
        {
        }

        public ServiceArea Get(string id)
        {
            return Read<ServiceArea>().FirstOrDefault(x => x.Id == id);
        }

        public List<ServiceArea> GetAll(string churchId)
        {
            return Read<ServiceArea>().Where(x => string.IsNullOrEmpty(x.ChurchId) || x.ChurchId == churchId).OrderBy(x => x.Name).ToList();
        }

        #region CRUD
        public Result<ServiceArea> Create(ServiceArea entity)
        {
            try
            {
                Create<ServiceArea>(entity);
                SaveChanges();
                return new Result<ServiceArea>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<ServiceArea>
                {
                    Data = entity,
                    Exception = ex,
                    Message = Constants.CreateExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<ServiceArea> Update(ServiceArea entity)
        {
            try
            {
                entity.ModifiedDate = DateTime.Now;
                entity.ModifiedBy = SessionVariables.CurrentUser.User.Id;

                Update<ServiceArea>(entity);
                SaveChanges();
                return new Result<ServiceArea>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<ServiceArea>
                {
                    Data = entity,
                    Exception = ex,
                    Message = Constants.UpdateExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<ServiceArea> Delete(ServiceArea entity)
        {
            try
            {
                Delete<ServiceArea>(entity);
                SaveChanges();
                return new Result<ServiceArea>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<ServiceArea>
                {
                    Data = entity,
                    Exception = ex,
                    Message = Constants.DeleteExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<ServiceArea> Delete(string id)
        {
            try
            {
                var entity = Get(id);
                Delete<ServiceArea>(entity);
                SaveChanges();
                return new Result<ServiceArea>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<ServiceArea>
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