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
    public class NotificationOperations : GenericRepository
    {
        public NotificationOperations(ApplicationDbContext db, Work work)
            : base(db, work)
        {
        }

        public Notification Get(string id)
        {
            return Read<Notification>().FirstOrDefault(x => x.Id == id);
        }

        public List<Notification> GetAll()
        {
            return Read<Notification>().ToList();
        }

        public List<Notification> GetAllByChurchId(string churchId)
        {
            return Read<Notification>().Where(x => x.ChurchId == churchId).ToList();
        }

        public List<Notification> GetAllByUser(string userId, int size)
        {
            var notifications = Read<Notification>().Where(x => x.AssignedToUserId == userId).OrderByDescending(q => q.CreatedDate).ToList();

            return notifications.Count >= size ? notifications.Take(size).ToList() : notifications;
        }

        #region CRUD
        public Result<Notification> Create(Notification entity)
        {
            try
            {
                Create<Notification>(entity);
                SaveChanges();
                return new Result<Notification>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<Notification>
                {
                    Data = entity,
                    Exception = ex,
                    Message = Constants.CreateExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<Notification> Create(IEnumerable<Notification> entities)
        {
            try
            {
                Create<Notification>(entities);
                SaveChanges();
                return new Result<Notification>
                {
                    ResultType = ResultType.Success,
                    List = entities.ToList()
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<Notification>
                {
                    List = entities.ToList(),
                    Exception = ex,
                    Message = Constants.CreateExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<Notification> Update(Notification entity)
        {
            try
            {
                entity.ModifiedDate = DateTime.Now;
                entity.ModifiedBy = SessionVariables.CurrentUser.User.Id;

                Update<Notification>(entity);
                SaveChanges();
                return new Result<Notification>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<Notification>
                {
                    Data = entity,
                    Exception = ex,
                    Message = Constants.UpdateExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<Notification> Delete(Notification entity)
        {
            try
            {
                Delete<Notification>(entity);
                SaveChanges();
                return new Result<Notification>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<Notification>
                {
                    Data = entity,
                    Exception = ex,
                    Message = Constants.DeleteExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<Notification> Delete(string id)
        {
            try
            {
                var entity = Get(id);
                Delete<Notification>(entity);
                SaveChanges();
                return new Result<Notification>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<Notification>
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