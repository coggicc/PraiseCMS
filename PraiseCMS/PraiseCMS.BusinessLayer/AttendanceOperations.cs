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
    public class AttendanceOperations : GenericRepository
    {
        public AttendanceOperations(ApplicationDbContext db, Work work)
         : base(db, work)
        {
        }

        public Attendance Get(string id)
        {
            return Read<Attendance>().FirstOrDefault(x => x.Id == id);
        }

        public List<Attendance> GetAll(string churchId)
        {
            return Read<Attendance>().Where(x => x.ChurchId == churchId).ToList();
        }

        public List<Attendance> GetAll(string churchId, DateRange dateRange)
        {
            var startDate = dateRange.StartDate.Date;
            var endDate = dateRange.EndDate.Date;

            return Read<Attendance>()
                .Where(x => x.ChurchId == churchId &&
                    ((x.OccurredOnDate.HasValue && DbFunctions.TruncateTime(x.OccurredOnDate.Value) >= startDate && DbFunctions.TruncateTime(x.OccurredOnDate.Value) <= endDate) ||
                     (!x.OccurredOnDate.HasValue && x.ModifiedDate.HasValue && DbFunctions.TruncateTime(x.ModifiedDate.Value) >= startDate && DbFunctions.TruncateTime(x.ModifiedDate.Value) <= endDate) ||
                     (!x.OccurredOnDate.HasValue && !x.ModifiedDate.HasValue && DbFunctions.TruncateTime(x.CreatedDate) >= startDate && DbFunctions.TruncateTime(x.CreatedDate) <= endDate)))
                .ToList();
        }

        public List<Attendance> GetAllByCampusId(string campusId)
        {
            return Read<Attendance>().Where(x => x.CampusId == campusId).ToList();
        }

        public WidgetsGraphModel GetGraphData(Church church)
        {
            var model = new WidgetsGraphModel();
            var allAttendance = GetAll(church.Id).Where(q => q.OccurredOnDate.IsNotNullOrEmpty()).ToList();
            model.Key = "attendance";

            var workWeekStartDay = ExtensionMethods.GetWorkWeekStartDay(church.WorkWeek);

            // Get the list of weeks based on the church's WorkWeek setting
            var weeks = ExtensionMethods.GetDatesOfLastNumberOfWeeks(DateTime.Now, 7, true, workWeekStartDay);

            foreach (var week in weeks)
            {
                var subModel = new GraphData
                {
                    // Sum the totals for the current week
                    Value = allAttendance
                                .Where(x => ((DateTime)x.OccurredOnDate).Date >= week.First() && ((DateTime)x.OccurredOnDate).Date <= week.Last())
                                .Select(s => s.Total)
                                .Sum()
                                .ToString(),

                    // Format the date range for the category
                    Category = $"{week.First():MMM dd} - {week.Last():MMM dd}"
                };
                model.Data.Add(subModel);
            }

            return model;
        }

        #region CRUD
        public Result<Attendance> Create(Attendance entity)
        {
            try
            {
                Create<Attendance>(entity);
                SaveChanges();
                return new Result<Attendance>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<Attendance>
                {
                    Data = entity,
                    Exception = ex,
                    Message = Constants.CreateExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<Attendance> Update(Attendance entity)
        {
            try
            {
                Update<Attendance>(entity);
                SaveChanges();
                return new Result<Attendance>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<Attendance>
                {
                    Data = entity,
                    Exception = ex,
                    Message = Constants.UpdateExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<Attendance> Delete(Attendance entity)
        {
            try
            {
                Delete<Attendance>(entity);
                SaveChanges();
                return new Result<Attendance>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<Attendance>
                {
                    Data = entity,
                    Exception = ex,
                    Message = Constants.DeleteExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<Attendance> Delete(string id)
        {
            try
            {
                var entity = Work.Attendance.Get(id);
                Delete<Attendance>(entity);
                SaveChanges();
                return new Result<Attendance>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<Attendance>
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