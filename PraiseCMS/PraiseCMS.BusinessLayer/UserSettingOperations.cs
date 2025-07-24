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
    public class UserSettingOperations : GenericRepository
    {
        public UserSettingOperations(ApplicationDbContext db, Work work)
         : base(db, work)
        {
        }

        public UserSetting Get(string id)
        {
            return Read<UserSetting>().FirstOrDefault(x => x.Id == id);
        }

        public UserSetting GetByUserId(string userId)
        {
            return Read<UserSetting>().FirstOrDefault(x => x.UserId == userId);
        }

        public List<UserSetting> GetByUserId(IEnumerable<string> userIds)
        {
            return Read<UserSetting>().Where(x => userIds.Contains(x.UserId)).ToList();
        }

        public List<UserSetting> GetByChurchId(string churchId)
        {
            return Read<UserSetting>().Where(x => x.PrimaryChurchId == churchId).ToList();
        }

        public Result<UserSetting> Create(UserSetting entity)
        {
            try
            {
                Create<UserSetting>(entity);
                SaveChanges();
                return new Result<UserSetting>
                {
                    Data = entity,
                    ResultIcon = ResultIcon.Success,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<UserSetting>
                {
                    Data = entity,
                    Exception = ex,
                    Message = Constants.CreateExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<UserSetting> Delete(UserSetting entity)
        {
            try
            {
                Delete<UserSetting>(entity);
                SaveChanges();
                return new Result<UserSetting>
                {
                    Data = entity,
                    ResultIcon = ResultIcon.Success,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<UserSetting>
                {
                    Data = entity,
                    Exception = ex,
                    Message = Constants.DeleteExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<UserSetting> Delete(string id)
        {
            try
            {
                var entity = Get(id);
                Delete<UserSetting>(entity);
                SaveChanges();
                return new Result<UserSetting>
                {
                    Data = entity,
                    ResultIcon = ResultIcon.Success,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<UserSetting>
                {
                    Exception = ex,
                    Message = Constants.DeleteExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<UserSetting> Update(UserSetting entity)
        {
            try
            {
                Update<UserSetting>(entity);
                SaveChanges();
                return new Result<UserSetting>
                {
                    Data = entity,
                    ResultIcon = ResultIcon.Success,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<UserSetting>
                {
                    Data = entity,
                    Exception = ex,
                    Message = Constants.UpdateExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }
    }
}