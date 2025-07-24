using PraiseCMS.BusinessLayer.Repository;
using PraiseCMS.DataAccess.DAL;
using PraiseCMS.DataAccess.Models;
using PraiseCMS.DataAccess.Models.ViewModels;
using PraiseCMS.DataAccess.Services;
using PraiseCMS.Shared.Methods;
using PraiseCMS.Shared.Models;
using PraiseCMS.Shared.Shared;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace PraiseCMS.BusinessLayer
{
    public class BaptismOperations : GenericRepository
    {
        public BaptismOperations(ApplicationDbContext db, Work work)
            : base(db, work)
        {
        }

        public Baptism Get(string id)
        {
            return Read<Baptism>().FirstOrDefault(x => x.Id.Equals(id));
        }

        public List<Baptism> GetAll(string churchId)
        {
            return Read<Baptism>().Where(x => x.ChurchId.Equals(churchId)).ToList();
        }

        public List<Baptism> GetAll(string churchId, DateRange dateRange)
        {
            var startDate = dateRange.StartDate.Date;
            var endDate = dateRange.EndDate.Date;

            return Read<Baptism>()
                .Where(x => x.ChurchId == churchId &&
                    ((x.OccurredOnDate.HasValue && DbFunctions.TruncateTime(x.OccurredOnDate.Value) >= startDate && DbFunctions.TruncateTime(x.OccurredOnDate.Value) <= endDate) ||
                     (!x.OccurredOnDate.HasValue && x.ModifiedDate.HasValue && DbFunctions.TruncateTime(x.ModifiedDate.Value) >= startDate && DbFunctions.TruncateTime(x.ModifiedDate.Value) <= endDate) ||
                     (!x.OccurredOnDate.HasValue && !x.ModifiedDate.HasValue && DbFunctions.TruncateTime(x.CreatedDate) >= startDate && DbFunctions.TruncateTime(x.CreatedDate) <= endDate)))
                .ToList();
        }

        public WidgetsGraphModel GetGraphData(Church church)
        {
            var model = new WidgetsGraphModel();
            var allBaptisms = GetAll(church.Id).Where(q => q.OccurredOnDate.IsNotNullOrEmpty()).ToList();
            model.Key = "baptism";

            var workWeekStartDay = ExtensionMethods.GetWorkWeekStartDay(church.WorkWeek);

            // Get the list of weeks based on the church's WorkWeek setting
            var weeks = ExtensionMethods.GetDatesOfLastNumberOfWeeks(DateTime.Now, 7, true, workWeekStartDay);

            foreach (var week in weeks)
            {
                var subModel = new GraphData
                {
                    Value = allBaptisms
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
        public Result<Baptism> Create(Baptism entity)
        {
            try
            {
                Create<Baptism>(entity);
                SaveChanges();
                return new Result<Baptism>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<Baptism>
                {
                    Data = entity,
                    Exception = ex,
                    Message = Constants.CreateExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<Baptism> Update(Baptism entity)
        {
            try
            {
                Update<Baptism>(entity);
                SaveChanges();
                return new Result<Baptism>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<Baptism>
                {
                    Data = entity,
                    Exception = ex,
                    Message = Constants.UpdateExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<Baptism> Delete(Baptism entity)
        {
            try
            {
                Delete<Baptism>(entity);
                SaveChanges();
                return new Result<Baptism>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<Baptism>
                {
                    Data = entity,
                    Exception = ex,
                    Message = Constants.DeleteExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<Baptism> Delete(string id)
        {
            try
            {
                var entity = Get(id);
                Delete<Baptism>(entity);
                SaveChanges();
                return new Result<Baptism>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<Baptism>
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