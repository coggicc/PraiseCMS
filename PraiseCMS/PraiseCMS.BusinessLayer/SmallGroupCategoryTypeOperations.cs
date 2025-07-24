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
    public class SmallGroupCategoryTypeOperations : GenericRepository
    {
        public SmallGroupCategoryTypeOperations(ApplicationDbContext db, Work work)
               : base(db, work)
        {
        }

        public SmallGroupCategoryType Get(string id)
        {
            return Read<SmallGroupCategoryType>().FirstOrDefault(x => x.Id == id);
        }

        public List<SmallGroupCategoryType> GetAll()
        {
            return Read<SmallGroupCategoryType>().OrderBy(x => x.ChurchId).ThenBy(x => x.Name).ToList();
        }

        #region CRUD
        public Result<SmallGroupCategoryType> Create(SmallGroupCategoryType entity)
        {
            try
            {
                Create<SmallGroupCategoryType>(entity);
                SaveChanges();
                return new Result<SmallGroupCategoryType>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<SmallGroupCategoryType>
                {
                    Data = entity,
                    Exception = ex,
                    Message = Constants.CreateExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<SmallGroupCategoryType> Update(SmallGroupCategoryType entity)
        {
            try
            {
                entity.ModifiedDate = DateTime.Now;
                entity.ModifiedBy = SessionVariables.CurrentUser.User.Id;

                Update<SmallGroupCategoryType>(entity);
                SaveChanges();
                return new Result<SmallGroupCategoryType>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<SmallGroupCategoryType>
                {
                    Data = entity,
                    Exception = ex,
                    Message = Constants.UpdateExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<SmallGroupCategoryType> Delete(SmallGroupCategoryType entity)
        {
            try
            {
                Delete<SmallGroupCategoryType>(entity);
                SaveChanges();
                return new Result<SmallGroupCategoryType>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<SmallGroupCategoryType>
                {
                    Data = entity,
                    Exception = ex,
                    Message = Constants.DeleteExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<SmallGroupCategoryType> Delete(string id)
        {
            try
            {
                var entity = Get(id);
                Delete<SmallGroupCategoryType>(entity);
                SaveChanges();
                return new Result<SmallGroupCategoryType>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<SmallGroupCategoryType>
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