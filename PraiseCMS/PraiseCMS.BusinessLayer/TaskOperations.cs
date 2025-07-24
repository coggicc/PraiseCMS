using PraiseCMS.BusinessLayer.Repository;
using PraiseCMS.DataAccess.DAL;
using PraiseCMS.DataAccess.Models;
using PraiseCMS.DataAccess.Models.ViewModels;
using PraiseCMS.DataAccess.Services;
using PraiseCMS.Shared.Methods;
using PraiseCMS.Shared.Shared;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PraiseCMS.BusinessLayer
{
    public class TaskOperations : GenericRepository
    {
        public TaskOperations(ApplicationDbContext db, Work work)
            : base(db, work)
        {
        }

        public TaskSD Get(string id)
        {
            return Read<TaskSD>().FirstOrDefault(x => x.Id == id);
        }

        public List<TaskSD> GetAll(string userId = null)
        {
            var tasksQuery = Read<TaskSD>();

            if (!string.IsNullOrEmpty(userId))
            {
                tasksQuery = tasksQuery.Where(x => x.AssignedToUserId == userId);
            }

            return tasksQuery.OrderByDescending(x => x.CreatedDate).ToList();
        }

        public List<TaskSD> GetAllByStatus(string status)
        {
            var now = DateTime.Now;
            IOrderedQueryable<TaskSD> query = Read<TaskSD>().OrderByDescending(x => x.CreatedDate);

            switch (status)
            {
                case TaskStatuses.Incomplete:
                    query = query.Where(x => !x.Completed) as IOrderedQueryable<TaskSD>;
                    break;

                case TaskStatuses.Complete:
                    query = query.Where(x => x.Completed) as IOrderedQueryable<TaskSD>;
                    break;

                case TaskStatuses.PastDue:
                    query = query.Where(x => x.DueDate < now && !x.Completed) as IOrderedQueryable<TaskSD>;
                    break;

                    // Default case: no filtering based on status
            }

            return query.ToList();
        }

        public DetailTasksViewModel GetDetails(string userId, TaskSD tasks)
        {
            var result = new DetailTasksViewModel();
            var task = Work.Task.Get(tasks.Id);

            if (tasks.Description.IsNotNullOrEmpty())
            {
                task.Description = tasks.Description;
                task.ModifiedBy = userId;
                task.ModifiedDate = DateTime.Now;
            }

            task.Completed = tasks.Completed;
            Work.Task.Update(task);

            result.Tasks = task;
            var userIds = new List<string> { result.Tasks.CreatedBy, result.Tasks.ModifiedBy }.Distinct().ToList();
            result.Users = Work.User.GetAll(userIds);
            return result;
        }

        #region CRUD
        public Result<TaskSD> Create(TaskSD entity)
        {
            try
            {
                Create<TaskSD>(entity);
                SaveChanges();
                return new Result<TaskSD>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<TaskSD>
                {
                    Data = entity,
                    Exception = ex,
                    Message = Constants.CreateExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<TaskSD> Update(string userId, TaskSD entity)
        {
            try
            {
                entity.ModifiedDate = DateTime.Now;
                entity.ModifiedBy = userId;

                Update<TaskSD>(entity);
                SaveChanges();
                return new Result<TaskSD>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<TaskSD>
                {
                    Data = entity,
                    Exception = ex,
                    Message = Constants.UpdateExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<TaskSD> Delete(TaskSD entity)
        {
            try
            {
                Delete<TaskSD>(entity);
                SaveChanges();
                return new Result<TaskSD>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<TaskSD>
                {
                    Data = entity,
                    Exception = ex,
                    Message = Constants.DeleteExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<TaskSD> Delete(string id)
        {
            try
            {
                var entity = Get(id);
                Delete<TaskSD>(entity);
                SaveChanges();
                return new Result<TaskSD>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<TaskSD>
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