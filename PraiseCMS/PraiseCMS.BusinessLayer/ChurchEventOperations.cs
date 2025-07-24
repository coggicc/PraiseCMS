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
    public class ChurchEventOperations : GenericRepository
    {
        public ChurchEventOperations(ApplicationDbContext db, Work work)
            : base(db, work)
        {
        }

        public ChurchEvent Get(string id)
        {
            var churchEvent = Read<ChurchEvent>().FirstOrDefault(x => x.Id == id);
            var eventType = Work.ChurchEventType.GetCustomName(churchEvent.ChurchEventTypeId);
            churchEvent.DisplayName = eventType.Type;

            return churchEvent;
        }

        public ChurchEvent GetAll(List<string> ids)
        {
            return Read<ChurchEvent>().FirstOrDefault(x => ids.Contains(x.Id));
        }

        public List<ChurchEventViewModel> GetAll(string churchId)
        {
            var listChurchEventVM = new List<ChurchEventViewModel>();
            var churchEventTypes = Work.ChurchEventType.GetAllViewModel(churchId, null, true);
            listChurchEventVM.AddRange(Read<ChurchEvent>().Where(x => x.ChurchId.Equals(churchId) && !x.IsDeleted).ToList().Select(q => new ChurchEventViewModel
            {
                Event = q,
                EventType = churchEventTypes.Find(x => x.Id.Equals(q.ChurchEventTypeId))
            }).ToList());

            return listChurchEventVM;
        }

        public List<ChurchEvent> GetAllByType(string typeIds)
        {
            var typeIdsList = typeIds.SplitToList().ToList();
            var types = Work.ChurchEventType.GetAllViewModel(SessionVariables.CurrentChurch.Id, typeIdsList, true);
            var events = Read<ChurchEvent>().Where(x => x.ChurchId.Equals(SessionVariables.CurrentChurch.Id) && !x.IsDeleted && typeIdsList.Contains(x.ChurchEventTypeId)).ToList();
            events.Select(x => { x.DisplayName = types.Find(q => q.Id.Equals(x.ChurchEventTypeId)).Type; return x; }).ToList();

            return events;
        }

        public List<ChurchEventType> GetAllByName(string name)
        {
            var model = new List<ChurchEventType>();

            if (string.IsNullOrEmpty(name))
            {
                return model;
            }

            var eventTypes = Read<ChurchEventType>().ToList();
            model.AddRange(eventTypes.Where(x => !string.IsNullOrEmpty(x.Type) && x.Type.ToUpper().Trim().Contains(name.ToUpper().Trim())));

            return model;
        }

        public DuplicateTimeModel GetAllCampusTimeByEvent(string eventId)
        {
            var model = new DuplicateTimeModel()
            {
                ChurchEvent = Get(id: eventId),
                ChurchEventSchedulers = GetAllScheduler(eventId: eventId)
            };

            if (model.ChurchEventSchedulers.Any())
            {
                var times = GetTimeBySchedulerId(model.ChurchEventSchedulers.Select(x => x.Id));
                //model.ChurchEventSchedulers.Select(x => { x.Times = times.FindAll(q => q.ChurchEventSchedulerId.Equals(x.Id)).OrderBy(o => Convert.ToDateTime(o.ShowEventAt)).ToList(); return x; }).ToList();
            }

            return model;
        }

        public ResponseModel DuplicateEventTime(string selectedId, string campusId, List<string> timesId)
        {
            var model = new ResponseModel();
            try
            {
                var schedulers = GetSchedulerIncludeTimes(selectedId);
                var newSchedulers = new ChurchEventScheduler()
                {
                    Id = Utilities.GenerateUniqueId(),
                    CreatedBy = SessionVariables.CurrentUser.User.Id,
                    CreatedDate = DateTime.Now,
                    CampusId = campusId,
                    EndDate = schedulers.EndDate,
                    SystemEndDate = schedulers.SystemEndDate,
                    EventEnds = schedulers.EventEnds,
                    EventId = schedulers.EventId,
                    EveryCount = schedulers.EveryCount,
                    EveryType = schedulers.EveryType,
                    Frequency = schedulers.Frequency,
                    IsDeleted = schedulers.IsDeleted,
                    Occurrences = schedulers.Occurrences,
                    RepeatOn = schedulers.RepeatOn,
                    StartDate = schedulers.StartDate
                };
                var times = schedulers.Times.Where(q => timesId.Contains(q.Id)).Select(x => new ChurchEventTime()
                {
                    Id = Utilities.GenerateUniqueId(),
                    CreatedDate = DateTime.Now,
                    CreatedBy = SessionVariables.CurrentUser.User.Id,
                    AllDay = x.AllDay,
                    //ChurchEventSchedulerId = newSchedulers.Id,
                    EndTime = x.EndTime,
                    HideEventAt = x.HideEventAt,
                    IsDeleted = x.IsDeleted,
                    ShowEventAt = x.ShowEventAt,
                    StartTime = x.StartTime,
                    EndDate = x.EndDate,
                }).ToList();
                CreateScheduler(newSchedulers);
                CreateTime(times);
                model.Success = true;

                return model;
            }
            catch (Exception ex)
            {
                model.Success = true; model.Message = ex.Message;
                return model;
            }
        }

        public ChurchEventDetail GetDetails(string id)
        {
            var model = new ChurchEventDetail
            {
                Event = Get(id),
                CampusWithTime = GetAllScheduler(eventId: id),
            };

            if (model.CampusWithTime.Any())
            {
                var times = GetTimeBySchedulerId(model.CampusWithTime.Select(x => x.Id));
                //model.CampusWithTime.Select(x => { x.Times = times.FindAll(q => q.ChurchEventSchedulerId.Equals(x.Id)).OrderBy(o => Convert.ToDateTime(o.ShowEventAt)).ToList(); return x; }).ToList();
            }

            //foreach (var campus in model.CampusIds)
            //{
            //    if (!model.CampusWithTime.Any(q => q.CampusId.Equals(campus)))
            //    {
            //        var churchEventScheduler = new ChurchEventScheduler
            //        {
            //            CampusId = campus,
            //            EventId = id,
            //            Id = null
            //        };
            //        model.CampusWithTime.Add(churchEventScheduler);
            //    }
            //}

            return model;
        }

        #region CRUD
        public Result<ChurchEvent> Create(ChurchEvent entity)
        {
            try
            {
                Create<ChurchEvent>(entity);
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
                    Message = Constants.UpdateExceptionMessage,
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

        public Result<ChurchEvent> Update(ChurchEvent entity)
        {
            try
            {
                entity.ModifiedBy = SessionVariables.CurrentUser.User.Id;
                entity.ModifiedDate = DateTime.Now;

                Update<ChurchEvent>(entity);
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
                    Message = Constants.UpdateExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }
        #endregion

        #region Event Scheduler
        public ResponseModel ModifySchedulerByEventId(string eventId, List<string> campusIds)
        {
            try
            {
                var schedulers = GetAllScheduler(eventId, includeTimes: true);
                var deletedCampuses = schedulers.Select(x => x.CampusId).Except(campusIds);
                var newCampuses = campusIds.Except(schedulers.Select(x => x.CampusId));
                var deletedSchedulers = schedulers.FindAll(x => deletedCampuses.Contains(x.CampusId)).ToList();
                deletedSchedulers.Select(x => { x.IsDeleted = true; x.Times.Select(t => { t.IsDeleted = true; return t; }); return x; }).ToList();
                var times = deletedSchedulers.SelectMany(x => x.Times).ToList();

                if (times.Any())
                {
                    UpdateTime(times);
                }

                if (deletedSchedulers.Any())
                {
                    UpdateScheduler(deletedSchedulers);
                }

                return new ResponseModel { Success = true };
            }
            catch (Exception ex)
            {
                return new ResponseModel { Success = true, Message = ex.Message };
            }
        }

        public List<ChurchEventScheduler> GetAllScheduler(string eventId, bool includeTimes = false)
        {
            var schedulerList = Read<ChurchEventScheduler>().Where(x => x.EventId.Equals(eventId) && !x.IsDeleted).ToList()
                .Select(x =>
                {
                    x.Campus = SessionVariables.Campuses.Find(q => q.Id.Equals(x.CampusId)).Display; return x;
                }).OrderBy(q => q.Campus).ToList();

            if (includeTimes)
            {
                var times = GetTimeBySchedulerId(schedulerList.Select(q => q.Id));
                //schedulerList.Select(x => { x.Times = times.FindAll(q => q.ChurchEventSchedulerId.Equals(x.Id)).ToList(); return x; }).ToList();
            }

            return schedulerList;
        }

        public ChurchEventScheduler GetScheduler(string id)
        {
            return Read<ChurchEventScheduler>().FirstOrDefault(x => x.Id == id);
        }

        public ChurchEventScheduler GetSchedulerIncludeTimes(string id, bool includeEvent = false)
        {
            var scheduler = Read<ChurchEventScheduler>().FirstOrDefault(x => x.Id == id);

            if (includeEvent)
            {
                scheduler.Event = Get(scheduler.EventId);
            }
            scheduler.Times = GetTimeBySchedulerId(scheduler.Id);

            return scheduler;
        }

        public ChurchEventScheduler GetSchedulerByCampusAndEventId(string campusId, string eventsId)
        {
            var scheduler = Read<ChurchEventScheduler>().FirstOrDefault(x => x.CampusId.Equals(campusId) && x.EventId.Equals(eventsId) && !x.IsDeleted);

            if (scheduler.IsNotNullOrEmpty())
            {
                scheduler.Times = GetTimeBySchedulerId(scheduler.Id);
            }

            return scheduler;
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

        public void CreateScheduler(List<ChurchEventScheduler> entities)
        {
            Create<ChurchEventScheduler>(entities);
            SaveChanges();
        }

        public void UpdateScheduler(ChurchEventScheduler entity)
        {
            entity.ModifiedDate = DateTime.Now;
            entity.ModifiedBy = SessionVariables.CurrentUser.User.Id;
            Update(entity);
            SaveChanges();
        }

        public void UpdateScheduler(IEnumerable<ChurchEventScheduler> entities)
        {
            entities.Select(q => { q.ModifiedBy = SessionVariables.CurrentUser.User.Id; q.ModifiedDate = DateTime.Now; return q; }).ToList();
            Update(entities);
            SaveChanges();
        }
        #endregion

        #region Event Time
        public ChurchEventTime GetTime(string id)
        {
            return Read<ChurchEventTime>().FirstOrDefault(x => x.Id == id);
        }

        public List<ChurchEventTime> GetTimeBySchedulerId(string id)
        {
            //return Read<ChurchEventTime>().Where(x => x.ChurchEventSchedulerId.Equals(id) && !x.IsDeleted).ToList();
            return Read<ChurchEventTime>().ToList();
        }

        public List<ChurchEventTime> GetTimeBySchedulerId(IEnumerable<string> ids)
        {
            //return Read<ChurchEventTime>().Where(x => ids.Contains(x.ChurchEventSchedulerId) && !x.IsDeleted).ToList();
            return Read<ChurchEventTime>().ToList();
        }

        public void DeleteTime(string id)
        {
            var entity = GetTime(id);
            Delete(entity);
            SaveChanges();
        }

        public void DeleteTime(ChurchEventTime entity)
        {
            Delete(entity);
            SaveChanges();
        }

        public void CreateTime(ChurchEventTime entity)
        {
            Create(entity);
            SaveChanges();
        }

        public void CreateTime(IEnumerable<ChurchEventTime> entity)
        {
            Create(entity);
            SaveChanges();
        }

        public void UpdateTime(ChurchEventTime entity)
        {
            entity.ModifiedDate = DateTime.Now;
            entity.ModifiedBy = SessionVariables.CurrentUser.User.Id;
            Update(entity);
            SaveChanges();
        }

        public void UpdateTime(IEnumerable<ChurchEventTime> entities)
        {
            entities.Select(q => { q.ModifiedBy = SessionVariables.CurrentUser.User.Id; q.ModifiedDate = DateTime.Now; return q; }).ToList();
            Update(entities);
            SaveChanges();
        }
        #endregion        

        public bool CheckCampusTime(string campusId, string eventId)
        {
            var scheduler = GetSchedulerByCampusAndEventId(campusId, eventId);
            return scheduler.IsNotNullOrEmpty() && scheduler.Times.Any();
        }

        public List<string> GetScheduledCampuses(string eventId)
        {
            var schedulers = GetAllScheduler(eventId, includeTimes: true);
            return schedulers.Where(x => x.Times.Any()).Select(c => c.CampusId).ToList();
        }

        #region CheckIns
        public List<CheckIn> GetAllCheckIns(string churchId, List<string> types = null)
        {
            if (types.IsNotNullOrEmpty())
            {
                return Read<CheckIn>().Where(x => x.ChurchId.Equals(churchId) && types.Contains(x.Type != null ? x.Type.ToLower() : string.Empty)).ToList();
            }

            return Read<CheckIn>().Where(x => x.ChurchId.Equals(churchId)).ToList();
        }

        public List<ChurchEventCheckIn> GetCheckInsModel(List<CheckIn> checkIns)
        {
            var userIds = checkIns.Where(x => x.CreatedBy.IsNotNullOrEmpty()).Select(x => x.CreatedBy).ToList();
            userIds.AddRange(checkIns.Where(x => x.ModifiedBy.IsNotNullOrEmpty()).Select(x => x.ModifiedBy));
            var users = Work.User.GetAll(userIds);
            var people = Work.Person.GetAllByPersonIds(SessionVariables.CurrentChurch.Id, checkIns.Select(x => x.PersonId));
            checkIns.Select(x =>
            {
                x.Person = people.Find(p => p.Id.Equals(x.PersonId)); x.CreatedByUser = users.Find(u => u.Id.Equals(x.CreatedBy));
                x.ModifiedByUser = users.Find(u => u.Id.Equals(x.ModifiedBy)); return x;
            }).ToList();

            var households = Work.Household.GetAllHouseholdsByPersonIds(checkIns.Select(x => x.PersonId));
            var model = new List<ChurchEventCheckIn>();

            foreach (var checkIn in checkIns)
            {
                var item = new ChurchEventCheckIn
                {
                    CheckIn = checkIn
                };
                var personHouseholds = households.Where(x => x.HouseholdMember.PersonId.Equals(checkIn.PersonId));

                if (personHouseholds.Any())
                {
                    foreach (var personHousehold in personHouseholds)
                    {
                        item.Households += item.Households.IsNullOrEmpty() ? $"<a href='/people/householdMembers/{personHousehold.Household.Id}'>{personHousehold.Household.Display}</a> ({personHousehold.HouseholdMember.FamilyRole})" : $", <a href='/people/householdMembers/{personHousehold.Household.Id}'>{personHousehold.Household.Display}</a> ({personHousehold.HouseholdMember.FamilyRole})";
                    }
                }

                model.Add(item);
            }

            return model;
        }

        public List<CheckIn> GetCheckInsByEvent(string churchId, string uniqeEventTimeId)
        {
            return Read<CheckIn>().Where(x => x.ChurchId.Equals(churchId) && x.ChurchEventTimeId.Equals(uniqeEventTimeId)).ToList();
        }
        #endregion
    }
}