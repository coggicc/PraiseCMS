using PraiseCMS.BusinessLayer.Repository;
using PraiseCMS.DataAccess.DAL;
using PraiseCMS.DataAccess.Models;
using PraiseCMS.DataAccess.Models.ViewModels;
using PraiseCMS.DataAccess.Services;
using PraiseCMS.DataAccess.Session;
using PraiseCMS.Shared.Methods;
using PraiseCMS.Shared.Models;
using PraiseCMS.Shared.Shared;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace PraiseCMS.BusinessLayer
{
    public class SalvationOperations : GenericRepository
    {
        public SalvationOperations(ApplicationDbContext db, Work work)
            : base(db, work)
        {
        }

        public Salvation Get(string id)
        {
            return Read<Salvation>().FirstOrDefault(x => x.Id == id);
        }

        public List<Salvation> GetAll(string churchId)
        {
            return Read<Salvation>().Where(x => x.ChurchId == churchId).ToList();
        }

        public List<Salvation> GetAll(string churchId, DateRange dateRange)
        {
            var startDate = dateRange.StartDate.Date;
            var endDate = dateRange.EndDate.Date;

            return Read<Salvation>()
                .Where(x => x.ChurchId == churchId &&
                    ((x.OccurredOnDate.HasValue && DbFunctions.TruncateTime(x.OccurredOnDate.Value) >= startDate && DbFunctions.TruncateTime(x.OccurredOnDate.Value) <= endDate) ||
                     (!x.OccurredOnDate.HasValue && x.ModifiedDate.HasValue && DbFunctions.TruncateTime(x.ModifiedDate.Value) >= startDate && DbFunctions.TruncateTime(x.ModifiedDate.Value) <= endDate) ||
                     (!x.OccurredOnDate.HasValue && !x.ModifiedDate.HasValue && DbFunctions.TruncateTime(x.CreatedDate) >= startDate && DbFunctions.TruncateTime(x.CreatedDate) <= endDate)))
                .ToList();
        }

        public List<Salvation> GetAllByCampusId(string campusId)
        {
            return Read<Salvation>().Where(x => x.CampusId == campusId).ToList();
        }

        public WidgetsGraphModel GetGraphData(Church church)
        {
            var model = new WidgetsGraphModel();
            var allSalvations = GetAll(church.Id).Where(q => q.OccurredOnDate.IsNotNullOrEmpty()).ToList();
            model.Key = "salvation";

            var workWeekStartDay = ExtensionMethods.GetWorkWeekStartDay(SessionVariables.CurrentChurch.WorkWeek);

            // Get the list of weeks based on the church's WorkWeek setting
            var weeks = ExtensionMethods.GetDatesOfLastNumberOfWeeks(DateTime.Now, 7, true, workWeekStartDay);

            foreach (var week in weeks)
            {
                var subModel = new GraphData
                {
                    Value = allSalvations
                        .Where(x => ((DateTime)x.OccurredOnDate).Date <= week.Last().Date && ((DateTime)x.OccurredOnDate).Date >= week.First().Date)
                        .Select(s => s.Total)
                        .Sum()
                        .ToString(),

                    Category = $"{week.First():MMM dd} - {week.Last():MMM dd}"
                };
                model.Data.Add(subModel);
            }

            return model;
        }

        #region CRUD
        public Result<Salvation> Create(Salvation entity)
        {
            try
            {
                Create<Salvation>(entity);
                SaveChanges();
                return new Result<Salvation>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<Salvation>
                {
                    Data = entity,
                    Exception = ex,
                    Message = Constants.CreateExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<Salvation> Update(Salvation entity)
        {
            try
            {
                entity.ModifiedDate = DateTime.Now;
                entity.ModifiedBy = SessionVariables.CurrentUser.User.Id;

                Update<Salvation>(entity);
                SaveChanges();
                return new Result<Salvation>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<Salvation>
                {
                    Data = entity,
                    Exception = ex,
                    Message = Constants.UpdateExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<Salvation> Delete(Salvation entity)
        {
            try
            {
                Delete<Salvation>(entity);
                SaveChanges();
                return new Result<Salvation>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<Salvation>
                {
                    Data = entity,
                    Exception = ex,
                    Message = Constants.DeleteExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<Salvation> Delete(string id)
        {
            try
            {
                var entity = Get(id);
                Delete<Salvation>(entity);
                SaveChanges();
                return new Result<Salvation>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<Salvation>
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