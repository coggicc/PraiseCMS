using PraiseCMS.BusinessLayer.Repository;
using PraiseCMS.DataAccess.DAL;
using PraiseCMS.DataAccess.Mapper;
using PraiseCMS.DataAccess.Models;
using PraiseCMS.DataAccess.Services;
using PraiseCMS.DataAccess.Shared;
using PraiseCMS.Shared.Methods;
using PraiseCMS.Shared.Models;
using PraiseCMS.Shared.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace PraiseCMS.BusinessLayer
{
    public class CampusOperations : GenericRepository
    {
        public CampusOperations(ApplicationDbContext db, Work work)
         : base(db, work)
        {
        }

        public Campus Get(string id)
        {
            return Read<Campus>().FirstOrDefault(x => x.Id == id);
        }

        public Campus GetByChurchId(string churchId)
        {
            return Read<Campus>().FirstOrDefault(x => x.ChurchId == churchId);
        }

        public string GetByEventId(string eventId)
        {
            var churchEvent = Work.ChurchEvent.Get(eventId);
            return null;
            //return churchEvent?.CampusId;
        }

        public List<Campus> GetAll(IEnumerable<string> ids)
        {
            return Read<Campus>().Where(x => ids.Contains(x.Id)).OrderBy(x => x.Name).ToList();
        }

        public List<Campus> GetAll(string churchId, DateRange dateRange = null)
        {
            var query = Read<Campus>().Where(x => x.ChurchId == churchId);

            if (dateRange != null)
            {
                query = query.Where(x => x.CreatedDate >= dateRange.StartDate && x.CreatedDate <= dateRange.EndDate);
            }

            return query.OrderBy(x => x.Name).ToList();
        }

        public CampusesView GetCampusesView(string churchId, DateRange dateRange)
        {
            var digital = Work.Giving.GetAll(churchId, dateRange);
            var offline = Work.OfflineGiving.GetAll(churchId, null, dateRange);
            var totalGiving = Mapper.Map(digital);
            totalGiving.AddRange(Mapper.Map(offline));

            return new CampusesView
            {
                Attendance = Work.Attendance.GetAll(churchId, dateRange),
                Campuses = Work.Campus.GetAll(churchId),
                //CheckIns = Work.ChurchEvent.GetAllCheckIns(churchId, dateRange),
                Payments = digital,
                PrayerRequests = Work.PrayerRequest.GetAll(churchId, dateRange),
                Salvations = Work.Salvation.GetAll(churchId, dateRange),
                SmallGroups = Work.SmallGroup.GetAll(churchId, dateRange),
                Users = Work.User.GetAllByChurchId(churchId),
                Giving = totalGiving
            };
        }

        public CampusDashboard GetDashboardById(string churchId, string id)
        {
            var model = new CampusDashboard
            {
                Campus = Work.Campus.Get(id),
                Attendance = Work.Attendance.GetAllByCampusId(id),
                CheckIns = Db.CheckIns.Where(x => x.CampusId == id).ToList(),
                Payments = Db.Payments.Where(x => x.CampusId == id).ToList(),
                PrayerRequests = Work.PrayerRequest.GetAllByCampusId(id).ToList(),
                Salvations = Work.Salvation.GetAllByCampusId(id),
                ServiceAreas = Work.ServiceArea.GetAll(churchId),
                SmallGroups = Work.SmallGroup.GetAllByCampusId(id),
                Users = Work.User.GetAllByChurchId(churchId).OrderByDescending(x => x.CreatedDate).Take(5).ToList()
            };

            return model;
        }

        public void CreateSettingsByCampusId(string churchId, string userId, string campusId)
        {
            Work.UserSetting.Create(
                new UserSetting
                {
                    Id = Utilities.GenerateUniqueId(),
                    UserId = userId,
                    PrimaryChurchId = churchId,
                    PrimaryChurchCampusId = campusId,
                    CreatedBy = userId,
                    CreatedDate = DateTime.Now,
                    DarkModeEnabled = false
                });
        }

        public CampusGivingDashboard GetGivingDashboard(string campusId, string churchId)
        {
            return new CampusGivingDashboard
            {
                Campus = Db.Campuses.Find(campusId),
                Giving = Db.Payments.Where(x => x.CampusId == campusId).OrderByDescending(x => x.CreatedDate).ToList(),
                Funds = Db.Funds.Where(x => x.ChurchId == churchId && !x.IsDeleted).ToList()
            };
        }

        public List<SelectListItem> GetCampusSelectList(List<Campus> campuses)
        {
            var campus = campuses.Select(q => new SelectListItem()
            {
                Text = q.Display,
                Value = q.Id
            }).OrderBy(x => x.Text).ToList();

            if (campus.IsNotNullOrEmpty() && campus.Any() && campus.Count == 1)
            {
                return campus.Select(d => { d.Selected = true; return d; }).ToList();
            }

            return campus;
        }

        #region CRUD        
        public Result<Campus> Create(Campus entity)
        {
            try
            {
                Create<Campus>(entity);
                SaveChanges();
                return new Result<Campus>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<Campus>
                {
                    Data = entity,
                    Exception = ex,
                    Message = Constants.CreateExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public void Create(List<Campus> campuss)
        {
            Create<Campus>(campuss);
            SaveChanges();
        }

        public Result<Campus> Update(string userId, Campus entity)
        {
            try
            {
                entity.ModifiedDate = DateTime.Now;
                entity.ModifiedBy = userId;

                Update<Campus>(entity);
                SaveChanges();
                return new Result<Campus>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<Campus>
                {
                    Data = entity,
                    Exception = ex,
                    Message = Constants.UpdateExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<Campus> Delete(Campus entity)
        {
            try
            {
                Delete<Campus>(entity);
                SaveChanges();
                return new Result<Campus>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<Campus>
                {
                    Data = entity,
                    Exception = ex,
                    Message = Constants.DeleteExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<Campus> Delete(string id)
        {
            try
            {
                var entity = Get(id);
                Delete<Campus>(entity);
                SaveChanges();
                return new Result<Campus>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<Campus>
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