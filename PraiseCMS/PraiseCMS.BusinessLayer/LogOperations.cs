using PraiseCMS.BusinessLayer.Repository;
using PraiseCMS.DataAccess.DAL;
using PraiseCMS.DataAccess.Models;
using PraiseCMS.DataAccess.Services;
using PraiseCMS.Shared.Methods;
using PraiseCMS.Shared.Shared;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PraiseCMS.BusinessLayer
{
    public class LogOperations : GenericRepository
    {
        public LogOperations(ApplicationDbContext db, Work work)
            : base(db, work)
        {
        }

        public Log Get(string id)
        {
            return Read<Log>().FirstOrDefault(x => x.Id == id);
        }

        public List<Log> GetAll()
        {
            return Read<Log>().ToList();
        }

        public List<Log> GetAllExceptions()
        {
            return Read<Log>().Where(x => x.Status == LogStatuses.Exception || x.Status == LogStatuses.Error).OrderByDescending(x => x.CreatedDate).ToList();
        }

        public List<Log> GetAllWithoutExceptions()
        {
            return Read<Log>().Where(x => x.Status != LogStatuses.Exception).OrderByDescending(x => x.CreatedDate).ToList();
        }

        public LogListViewModel GetAll(string logType, string sortType = SortOrders.Descending, string church = null, string controller = null, string type = null, int pageNumber = 1, int pageSize = 50)
        {
            var result = new LogListViewModel();
            var logs = Read<Log>().ToList();

            #region filter dropdowns
            var churches = Work.Church.GetAll(logs.Select(x => x.ChurchId).Distinct().ToList());
            var controllerList = logs.Select(x => x.Controller).Distinct().ToList();
            var typeList = logs.Select(x => x.Type).Distinct().ToList();

            foreach (var item in churches)
            {
                result.ChurchList.Add(new SelectListItems { Text = item.Name, Value = item.Id });
            }

            foreach (var item in controllerList)
            {
                result.ControllerList.Add(new SelectListItems { Text = item, Value = item });
            }

            foreach (var item in typeList)
            {
                result.TypeList.Add(new SelectListItems { Text = item, Value = item });
            }
            #endregion

            #region filters
            logs = logType == LogType.Inbox ? logs.Where(x => x.Status == LogStatuses.Done).ToList() : logs.Where(x => x.Status != LogStatuses.Done).ToList();

            logs = sortType == SortOrders.Ascending ? logs.OrderBy(x => x.CreatedDate).ToList() : logs.OrderByDescending(x => x.CreatedDate).ToList();

            if (church.IsNotNullOrEmpty())
            {
                logs = logs.Where(x => x.ChurchId == church).ToList();
            }

            if (controller.IsNotNullOrEmpty())
            {
                logs = logs.Where(x => x.Controller == controller).ToList();
            }

            if (type.IsNotNullOrEmpty())
            {
                logs = logs.Where(x => x.Type == type).ToList();
            }
            #endregion

            result.TotalLogs = logs.Count;
            result.Church = church;
            result.Controller = controller;
            result.Type = type;
            result.Logs = logs.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
            result.Page = pageNumber;
            var pageCount = Convert.ToDouble(result.TotalLogs) / Convert.ToDouble(pageSize);
            result.TotalPage = Convert.ToInt32(Math.Ceiling(pageCount));

            #region pagination and sorting
            if (pageNumber == 1)
            {
                result.From = 1;
                result.To = pageSize;
            }
            else
            {
                result.From = pageSize * pageNumber;
                result.To = (pageSize * (pageNumber - 1)) + 1;
            }

            result.To = result.TotalLogs < result.To ? result.TotalLogs : result.To;

            if (sortType == SortOrders.Descending)
            {
                result.Newest = "active";
                result.Oldest = string.Empty;
            }
            else if (sortType == SortOrders.Ascending)
            {
                result.Newest = string.Empty;
                result.Oldest = "active";
            }
            #endregion

            return result;
        }

        #region CRUD
        public Result<Log> Create(Log entity)
        {
            try
            {
                Create<Log>(entity);
                SaveChanges();
                return new Result<Log>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<Log>
                {
                    Data = entity,
                    Exception = ex,
                    Message = Constants.CreateExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<Log> Delete(Log entity)
        {
            try
            {
                Delete<Log>(entity);
                SaveChanges();
                return new Result<Log>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<Log>
                {
                    Data = entity,
                    Exception = ex,
                    Message = Constants.DeleteExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<Log> Delete(string id)
        {
            try
            {
                var entity = Get(id);
                Delete<Log>(entity);
                SaveChanges();
                return new Result<Log>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<Log>
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