using PraiseCMS.BusinessLayer.Repository;
using PraiseCMS.DataAccess.DAL;
using PraiseCMS.DataAccess.Models;
using PraiseCMS.DataAccess.Models.ViewModels;
using PraiseCMS.DataAccess.Services;
using PraiseCMS.DataAccess.Session;
using PraiseCMS.DataAccess.Shared;
using PraiseCMS.Shared.Methods;
using PraiseCMS.Shared.Shared;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PraiseCMS.BusinessLayer
{
    public class ChurchEventOperationsOLD : GenericRepository
    {
        public ChurchEventOperationsOLD(ApplicationDbContext db, Work work)
            : base(db, work)
        {
        }

        //public List<ChurchEvent> GetEventByName(string name)
        //{
        //    var model = new List<ChurchEvent>();
        //    if (string.IsNullOrEmpty(name))
        //    {
        //        return model;
        //    }
        //    var events = Read<ChurchEvent>().Where(x => x.ChurchId.Equals(SessionVariables.CurrentChurch.Id)).ToList();
        //    model.AddRange(events.Where(x => !string.IsNullOrEmpty(x.Name) ? x.Name.ToUpper().Trim().Contains(name.ToUpper().Trim()) : false));
        //    model.AddRange(events.Where(x => !model.Select(q => q.Id).Contains(x.Id) && !string.IsNullOrEmpty(x.CustomName) ? x.CustomName.ToUpper().Trim().Contains(name.ToUpper().Trim()) : false));
        //    return model;
        //}

        public ChurchEvent Get(string id)
        {
            return Read<ChurchEvent>().FirstOrDefault(x => x.Id == id);
        }
        //public List<CheckIn> GetAllCheckIns(string EventId, string churchId)
        //{
        //    return Read<CheckIn>().Where(x => x.ChurchId == churchId && x.ChurchEventId == EventId).ToList();
        //}
        //public List<CheckIn> GetAllCheckIns(string churchId, DateRange dateRange)
        //{
        //    return Read<CheckIn>().Where(x => x.ChurchId == churchId && x.CreatedDate >= dateRange.StartDate && x.CreatedDate <= dateRange.EndDate).ToList();
        //}
        //public bool IsCheckedIn(CheckIn CheckIn)
        //{
        //    return Read<CheckIn>().Where(x => x.TypeId == CheckIn.TypeId && x.ChurchEventId == CheckIn.ChurchEventId).Any();
        //}

        public void CheckIn(CheckIn CheckIn)
        {
            string type = "Guest";
            var role = DAL.ReadRoleByUserId(CheckIn.TypeId);        //This needs to be updated.  The types of check-ins are Guests & Volunteers only for now.

            if (role != null)
            {
                type = role.Name;
            }

            CheckIn.Id = Utilities.GenerateUniqueId();
            CheckIn.CampusId = SessionVariables.CurrentCampus.Id;
            CheckIn.CreatedBy = SessionVariables.CurrentUser.User.Id;
            CheckIn.CreatedDate = DateTime.Now;
            CheckIn.ChurchId = SessionVariables.CurrentChurch.Id;
            CheckIn.Type = type;

            Create(CheckIn);
            SaveChanges();
        }

        public void CheckInUpdate(string checkInId)
        {
            var result = Read<CheckIn>().FirstOrDefault(x => x.Id == checkInId);
            result.ModifiedBy = SessionVariables.CurrentUser.User.Id;
            result.ModifiedDate = DateTime.Now;
            SaveChanges();
        }

        public ChurchEventWithSchedulerVM GetWithScheduler(string eventId)
        {
            var result = new ChurchEventWithSchedulerVM
            {
                ChurchEvent = Work.ChurchEvent.Get(eventId),
                ChurchEventScheduler = Work.ChurchEvent.GetAllScheduler(eventId)
            };

            if (result.ChurchEventScheduler.Any())
            {
                var campusIds = result.ChurchEventScheduler.Select(x => x.CampusId).ToArray().CombineToString(",");
                result.Campuses = Read<Campus>().Where(x => campusIds.Contains(x.Id)).ToList();
            }

            return result;
        }

        public ChurchEventScheduler GetChurchEventScheduler(string id)
        {
            return Read<ChurchEventScheduler>().FirstOrDefault(x => x.Id == id);
        }

        public void UpdateEventTime(EventTimeViewModel model)
        {
            model.ChurchEventScheduler.ModifiedDate = DateTime.Now;
            model.ChurchEventScheduler.ModifiedBy = SessionVariables.CurrentUser.User.Id;

            if (model.Campuses.Any())
            {
                model.ChurchEventScheduler.CampusId = model.Campuses.CombineListToString(",");
            }

            UpdateScheduler(model.ChurchEventScheduler);
        }

        public void UpdateChurchEventScheduler(EventTimeViewModel model)
        {
            if (!string.IsNullOrEmpty(model.ChurchEventScheduler.Id))
            {
                var result = GetScheduler(model.ChurchEventScheduler.Id);
                result.CampusId = model.Campuses.Contains("All") ? "All" : model.Campuses.ToArray().CombineToString(",");
                result.ModifiedBy = SessionVariables.CurrentUser.User.Id;
                result.ModifiedDate = DateTime.Now;
                UpdateScheduler(result);
            }
            else
            {
                model.ChurchEventScheduler.CampusId = model.Campuses.Contains("All") ? "All" : model.Campuses.ToArray().CombineToString(",");
                model.ChurchEventScheduler.Id = Utilities.GenerateUniqueId();
                model.ChurchEventScheduler.CreatedBy = SessionVariables.CurrentUser.User.Id;
                model.ChurchEventScheduler.CreatedDate = DateTime.Now;
                CreateScheduler(model.ChurchEventScheduler);
            }
        }

        public void AddFirstEvent(string eventId, string eventStartDate, string eventStartTime, string[] Campuses)
        {
            if (!string.IsNullOrEmpty(eventId) && !string.IsNullOrEmpty(eventStartDate) && !string.IsNullOrEmpty(eventStartTime) && Campuses.Length > 0)
            {
                foreach (var campusId in Campuses)
                {
                    if (!string.IsNullOrEmpty(campusId))
                    {
                        var model = new ChurchEventScheduler
                        {
                            Id = Utilities.GenerateUniqueId(),
                            EventId = eventId,
                            CampusId = campusId,
                            StartDate = Convert.ToDateTime(eventStartDate),
                            CreatedDate = DateTime.Now,
                            CreatedBy = SessionVariables.CurrentUser.User.Id
                        };

                        CreateScheduler(model);
                    }
                }
            }
        }

        public Result<ChurchEventView> Create(ChurchEventView churchEventView)
        {
            try
            {
                churchEventView.Campuses = churchEventView.Campuses.Where(x => !x.Contains("All")).OrderBy(x => x).ToList();
                var campusList = churchEventView.Campuses.CombineListToString(",");

                Create(new ChurchEvent
                {
                    //CampusId = campusList,
                    ChurchId = SessionVariables.CurrentChurch.Id,
                    CreatedBy = SessionVariables.CurrentUser.User.Id,
                    CreatedDate = DateTime.Now,
                    Id = Utilities.GenerateUniqueId(),
                    ChurchEventTypeId = churchEventView.CurrentChurchEvent.ChurchEventTypeId
                });
                SaveChanges();

                return new Result<ChurchEventView>
                {
                    Data = churchEventView,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<ChurchEventView>
                {
                    Data = churchEventView,
                    Exception = ex,
                    Message = Constants.CreateExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<ChurchEvent> Delete(ChurchEvent entity)
        {
            try
            {
                Delete<ChurchEvent>(entity);
                SaveChanges();
                return new Result<ChurchEvent>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<ChurchEvent>
                {
                    Data = entity,
                    Exception = ex,
                    Message = Constants.DeleteExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<ChurchEvent> Delete(string id)
        {
            try
            {
                var entity = Get(id);
                Delete<ChurchEvent>(entity);
                SaveChanges();
                return new Result<ChurchEvent>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<ChurchEvent>
                {
                    Exception = ex,
                    Message = Constants.DeleteExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<ChurchEventView> Update(ChurchEventView churchEventView)
        {
            try
            {
                churchEventView.Campuses = churchEventView.Campuses.Where(x => !x.Contains("All")).OrderBy(x => x).ToList();
                var campusList = churchEventView.Campuses.CombineListToString(",");

                var evnt = Read<ChurchEvent>().FirstOrDefault(x => x.Id == churchEventView.CurrentChurchEvent.Id);
                //evnt.CampusId = campusList;
                evnt.ChurchEventTypeId = churchEventView.CurrentChurchEvent.ChurchEventTypeId;
                evnt.ModifiedBy = SessionVariables.CurrentUser.User.Id;
                evnt.ModifiedDate = DateTime.Now;

                Update(evnt);
                SaveChanges();

                return new Result<ChurchEventView>
                {
                    Data = churchEventView,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<ChurchEventView>
                {
                    Data = churchEventView,
                    Exception = ex,
                    Message = Constants.UpdateExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        #region Event Scheduler
        public List<ChurchEventScheduler> GetAllScheduler(string eventId)
        {
            return Read<ChurchEventScheduler>().Where(x => x.EventId == eventId).ToList();
        }

        public ChurchEventScheduler GetScheduler(string id)
        {
            return Read<ChurchEventScheduler>().FirstOrDefault(x => x.Id == id);
        }

        public void DeleteScheduler(string id)
        {
            var entity = GetScheduler(id);
            Delete(entity);
            SaveChanges();
        }

        public void DeleteScheduler(ChurchEventScheduler entity)
        {
            Delete(entity);
            SaveChanges();
        }

        public void CreateScheduler(ChurchEventScheduler entity)
        {
            Create(entity);
            SaveChanges();
        }

        public void UpdateScheduler(ChurchEventScheduler entity)
        {
            Update(entity);
            SaveChanges();
        }
        #endregion
    }
}