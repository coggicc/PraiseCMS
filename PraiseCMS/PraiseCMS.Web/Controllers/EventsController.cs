using PraiseCMS.DataAccess.Models;
using PraiseCMS.DataAccess.Session;
using PraiseCMS.DataAccess.Shared;
using PraiseCMS.Shared.Methods;
using PraiseCMS.Shared.Models;
using PraiseCMS.Shared.Shared;
using PraiseCMS.Web.Attributes;
using PraiseCMS.Web.Controllers.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using System.Windows.Controls;

namespace PraiseCMS.Web.Controllers
{
    [RequireUser]

    [RequirePermission(ModuleId = "72142377372b70c1df42a948a08c39")]
    public class EventsController : BaseController
    {
        public ActionResult Index()
        {
            var dashboard = new EventDashboard()
            {
                UpComingEvents = work.Event.GetUpComingEvents(SessionVariables.CurrentChurch.Id),
                EventsByDate = new EventsByDate
                {
                    Date = DateTime.Now,
                    Events = work.Event.GetEvents(SessionVariables.CurrentChurch.Id, $"{DateTime.Now.ToShortDateString()}-{DateTime.Now.ToShortDateString()}")
                }
            };
            return View(dashboard);
        }

        [RequirePermission(ModuleId = "79308018387bddf86d2cd6497890e9")]

        public ActionResult Manage()
        {
            var result = work.ChurchEvents.GetAll(SessionVariables.CurrentChurch.Id);
            return View(result);
        }

        public ActionResult GetDashboard(DateTime date, string campusId)
        {
            DateRange dateRange = date.ToDateRange();

            var dashboard = new EventDashboard()
            {
                UpComingEvents = work.Event.GetUpComingEvents(SessionVariables.CurrentChurch.Id),
                EventsByDate = new EventsByDate
                {
                    Date = date,
                    Events = work.Event.GetEvents(SessionVariables.CurrentChurch.Id, $"{dateRange.StartDate.ToShortDateString()}-{dateRange.EndDate.ToShortDateString()}")
                }
            };

            if (campusId.IsNotNullOrEmpty())
            {
                dashboard.UpComingEvents = dashboard.UpComingEvents.FindAll(x => x.CampusId.Equals(campusId));
                dashboard.EventsByDate.Events = dashboard.EventsByDate.Events.FindAll(x => x.CampusId.Equals(campusId));
            }

            return PartialView("_Dashboard", dashboard);
        }

        public ActionResult GetEventsByDate(DateTime date, string campusId)
        {
            DateRange dateRange = date.ToDateRange();

            var model = new EventsByDate
            {
                Date = date,
                Events = work.Event.GetEvents(SessionVariables.CurrentChurch.Id, $"{dateRange.StartDate.ToShortDateString()}-{dateRange.EndDate.ToShortDateString()}")
            };

            if (campusId.IsNotNullOrEmpty())
            {
                model.Events = model.Events.FindAll(x => x.CampusId.Equals(campusId));
            }

            return PartialView("_Events", model);
        }

        public ActionResult Overview(string id, string day, string defaultselected)
        {
            ViewBag.DefaultSelectedId = defaultselected;
            return View(work.Event.GetEventOverView(SessionVariables.CurrentChurch.Id, id, day));
        }

        public ActionResult Details(string id)
        {
            var model = work.ChurchEvents.GetDetails(id);
            return View(model);
        }

        [HttpGet]
        public ActionResult _CreateChurchEvent()
        {
            var churchEventView = new ChurchEventViewModel
            {
                Event = new ChurchEvent()
                {
                    Id = Utilities.GenerateUniqueId(),
                    ChurchId = SessionVariables.CurrentChurch.Id,
                    CreatedBy = SessionVariables.CurrentUser.User.Id,
                    CreatedDate = DateTime.Now
                },
                EventTypes = work.ChurchEventType.GetAllViewModel(SessionVariables.CurrentChurch.Id, null, true)
            };

            return PartialView("_CreateEdit", churchEventView);
        }

        [HttpPost]
        public ActionResult _CreateChurchEvent(ChurchEventViewModel model)
        {
            if (!ModelState.IsValid || model.Campuses.IsNullOrEmpty() || !model.Campuses.Any())
            {
                if (model.Campuses.IsNullOrEmpty() || !model.Campuses.Any())
                {
                    CreateAlertMessage("Please select at least one campus", AlertMessageTypes.Warning, AlertMessageIcons.Warning);
                }

                model.EventTypes = work.ChurchEventType.GetAllViewModel(SessionVariables.CurrentChurch.Id, null, true);

                return PartialView("_CreateEdit", model);
            }

            var existingEvents = work.ChurchEvents.GetAll(SessionVariables.CurrentChurch.Id);

            if (existingEvents.Select(x => x.Event.ChurchEventTypeId).Contains(model.Event.ChurchEventTypeId))
            {
                CreateAlertMessage("An event with this name already exists. You may add a new time or campus, or you must choose a different name for your event.", AlertMessageTypes.Warning, AlertMessageIcons.Warning);
                model.EventTypes = work.ChurchEventType.GetAllViewModel(SessionVariables.CurrentChurch.Id, null, true);
                return PartialView("_CreateEdit", model);
            }

            //model.Event.CampusId = string.Join(",", model.Campuses);
            work.ChurchEvents.Create(model.Event);

            return AjaxRedirectTo($"/events/details/{model.Event.Id}");
        }

        [HttpGet]
        public ActionResult _EditChurchEvent(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var churchEvent = work.ChurchEvents.Get(id);

            if (churchEvent == null)
            {
                return HttpNotFound();
            }

            var churchEventViewModel = new ChurchEventViewModel
            {
                Event = churchEvent,
                EventTypes = work.ChurchEventType.GetAllViewModel(SessionVariables.CurrentChurch.Id, null, true)
            };

            return PartialView("_CreateEdit", churchEventViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _EditChurchEvent(ChurchEventViewModel model)
        {
            if (!ModelState.IsValid || model.Campuses.IsNullOrEmpty() || !model.Campuses.Any())
            {
                if (model.Campuses.IsNullOrEmpty() || !model.Campuses.Any())
                {
                    CreateAlertMessage("Please select at least one campus", AlertMessageTypes.Warning, AlertMessageIcons.Warning);
                }

                model.EventTypes = work.ChurchEventType.GetAllViewModel(SessionVariables.CurrentChurch.Id, null, true);

                return PartialView("_CreateEdit", model);
            }

            var existingEvents = work.ChurchEvents.GetAll(SessionVariables.CurrentChurch.Id).Where(x => !x.Event.Id.Equals(model.Event.Id));

            if (existingEvents.Select(x => x.Event.ChurchEventTypeId).Contains(model.Event.ChurchEventTypeId))
            {
                CreateAlertMessage("An event with this name already exists. You may add a new time or campus, or you must choose a different name for your event.", AlertMessageTypes.Warning, AlertMessageIcons.Warning);
                model.EventTypes = work.ChurchEventType.GetAllViewModel(SessionVariables.CurrentChurch.Id, null, true);

                return PartialView("_CreateEdit", model);
            }

            //model.Event.CampusId = string.Join(",", model.Campuses);
            work.ChurchEvents.ModifySchedulerByEventId(model.Event.Id, model.Campuses);

            var churchEvent = work.ChurchEvents.Get(model.Event.Id);
            churchEvent.Id = model.Event.Id;
            churchEvent.IsDeleted = model.Event.IsDeleted;
            churchEvent.ModifiedBy = model.Event.ModifiedBy;
            churchEvent.ModifiedDate = model.Event.ModifiedDate;
            churchEvent.CreatedBy = model.Event.CreatedBy;
            churchEvent.CreatedDate = model.Event.CreatedDate;
            //churchEvent.CampusId = model.Event.CampusId;
            churchEvent.ChurchEventTypeId = model.Event.ChurchEventTypeId;
            churchEvent.ChurchId = model.Event.ChurchId;
            work.ChurchEvents.Update(churchEvent);

            return AjaxRedirectTo($"/events/Details/{model.Event.Id}");
        }

        [HttpGet]
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var churchEvent = work.ChurchEvents.Get(id);
            churchEvent.IsDeleted = true;

            work.ChurchEvents.Update(churchEvent);
            CreateAlertMessage("The event has been deleted.", AlertMessageTypes.Success, AlertMessageIcons.Success);

            return RedirectToAction("manage");
        }

        #region ChurchEventScheduler  

        [HttpGet]
        public ActionResult _CreateEventTime(string eventId, string campusId)
        {
            var time = new ChurchEventScheduler()
            {
                Id = Utilities.GenerateUniqueId(),
                EventId = eventId,
                CreatedBy = SessionVariables.CurrentUser.User.Id,
                CreatedDate = DateTime.Now,
                StartDate = DateTime.Now,
                CampusId = campusId,
                Time = new ChurchEventTime
                {
                    AllDay = false,
                    //StartTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour + 1, 0, 0).ToString("hh:mm tt"),
                    //EndTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour + 2, 0, 0).ToString("hh:mm tt"),
                    ShowEventAt = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, 0, 0).ToString("hh:mm tt"),
                    HideEventAt = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour + 2, 30, 0).ToString("hh:mm tt")
                },
                Frequency = ChurchEventFrequency.DoesNotRepeat,
                EventEnds = EventEnds.Never
            };

            return PartialView("_CreateEditEventScheduler", time);
        }

        [HttpPost]
        public ActionResult _CreateEventTime(ChurchEventScheduler model)
        {
            if (ModelState.IsValid)
            {
                if (model.Time.AllDay)
                {
                    //model.Time.EndTime = model.Time.ShowEventAt = model.Time.HideEventAt = model.Time.StartTime = null;
                }

                model.Time.CreatedBy = SessionVariables.CurrentUser.User.Id;
                model.Time.CreatedDate = DateTime.Now;
                model.Time.Id = Utilities.GenerateUniqueId();
                //model.Time.ChurchEventSchedulerId = model.Id;

                if (model.Frequency.EqualsIgnoreCase(ChurchEventFrequency.DoesNotRepeat))
                {
                    model.EventEnds = EventEnds.OnSpecificDate;
                    model.EndDate = model.StartDate;
                }

                if (model.EventEnds.EqualsIgnoreCase(EventEnds.Never))
                {
                    model.EndDate = null; model.Occurrences = null;
                    model.SystemEndDate = DateTime.Now.AddYears(1);
                }
                else if (model.EventEnds.EqualsIgnoreCase(EventEnds.OnSpecificDate))
                {
                    model.Occurrences = null;
                }
                else if (model.EventEnds.EqualsIgnoreCase(EventEnds.AfterEventOccurrences))
                {
                    model.EndDate = null;
                }

                if (model.Frequency.EqualsIgnoreCase(ChurchEventFrequency.Custom))
                {
                    if (model.EveryType.ContainsIgnoreCase(EventRepeatFrequency.Day.ToLower()) || model.EveryType.ContainsIgnoreCase(EventRepeatFrequency.Year.ToLower()))
                    {
                        model.RepeatOn = null;
                    }
                }
                else
                {
                    model.EveryType = null; model.EveryCount = null; model.RepeatOn = null;
                }

                work.ChurchEvents.CreateScheduler(model);
                work.ChurchEvents.CreateTime(model.Time);

                return RedirectToAction(nameof(Details), new { id = model.EventId });
            }
            DisplayErrors();

            return RedirectToAction(nameof(Details), new { id = model.EventId });
        }

        [HttpGet]
        public ActionResult _EditEventTime(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var time = work.ChurchEvents.GetScheduler(id);
            time.Time = new ChurchEventTime();

            return PartialView("_CreateEditEventScheduler", time);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _EditEventTime(ChurchEventScheduler model)
        {
            if (ModelState.IsValid)
            {
                if (model.Frequency.EqualsIgnoreCase(ChurchEventFrequency.DoesNotRepeat))
                {
                    model.EventEnds = EventEnds.OnSpecificDate;
                    model.EndDate = model.StartDate;
                }

                if (model.EventEnds.EqualsIgnoreCase(EventEnds.Never))
                {
                    model.EndDate = null; model.Occurrences = null;
                }
                else if (model.EventEnds.EqualsIgnoreCase(EventEnds.OnSpecificDate))
                {
                    model.Occurrences = null;
                }
                else if (model.EventEnds.EqualsIgnoreCase(EventEnds.AfterEventOccurrences))
                {
                    model.EndDate = null;
                }

                if (model.Frequency.EqualsIgnoreCase(ChurchEventFrequency.Custom))
                {
                    if (model.EveryType.ContainsIgnoreCase(EventRepeatFrequency.Day.ToLower()) || model.EveryType.ContainsIgnoreCase(EventRepeatFrequency.Year.ToLower()))
                    {
                        model.RepeatOn = null;
                    }
                }
                else
                {
                    model.EveryType = null; model.EveryCount = null; model.RepeatOn = null;
                }

                work.ChurchEvents.UpdateScheduler(model);

                return RedirectToAction(nameof(Details), new { id = model.EventId });
            }

            DisplayErrors();

            return RedirectToAction(nameof(Details), new { id = model.EventId });
        }

        public ActionResult DeleteScheduler(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var scheduler = work.ChurchEvents.GetScheduler(id);
            var churchEvent = work.ChurchEvents.Get(scheduler.EventId);
            scheduler.IsDeleted = true;
            var times = work.ChurchEvents.GetTimeBySchedulerId(scheduler.Id);

            if (times.Any())
            {
                times.Select(x => { x.IsDeleted = true; return x; }).ToList();
            }

            work.ChurchEvents.UpdateTime(times);
            work.ChurchEvents.UpdateScheduler(scheduler);

            //if (churchEvent.IsNotNullOrEmpty() && churchEvent.CampusId.IsNotNullOrEmpty())
            //{
            //    var campuses = churchEvent.CampusId.SplitToList();
            //    churchEvent.CampusId = string.Join(",", campuses.Where(q => !q.Equals(scheduler.CampusId)));
            //}

            work.ChurchEvents.Update(churchEvent);
            CreateAlertMessage($"Event have been deleted for {SessionVariables.Campuses.Find(x => x.Id.Equals(scheduler.CampusId)).Display}.", AlertMessageTypes.Success, AlertMessageIcons.Success);

            return RedirectToAction(nameof(Details), new { id = scheduler.EventId });
        }

        #endregion

        #region ChurchEventTime
        [HttpGet]
        public ActionResult AddTime(string ChurchEventSchedulerId)
        {
            var time = new ChurchEventTime
            {
                Id = Utilities.GenerateUniqueId(),
                CreatedBy = SessionVariables.CurrentUser.User.Id,
                CreatedDate = DateTime.Now,
                AllDay = false,
                //ChurchEventSchedulerId = ChurchEventSchedulerId,
                //StartTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour + 1, 0, 0).ToString("hh:mm tt"),
                //EndTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour + 2, 0, 0).ToString("hh:mm tt"),
                ShowEventAt = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, 0, 0).ToString("hh:mm tt"),
                HideEventAt = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour + 2, 30, 0).ToString("hh:mm tt"),
                ShowMultiday = !work.ChurchEvents.GetTimeBySchedulerId(ChurchEventSchedulerId).Any()
            };

            return PartialView("_CreateEditTime", time);
        }

        [HttpPost]
        public ActionResult AddTime(ChurchEventTime model)
        {
            if (ModelState.IsValid)
            {
                if (model.AllDay)
                {
                    //model.EndTime = model.ShowEventAt = model.HideEventAt = model.StartTime = null;
                }

                work.ChurchEvents.CreateTime(model);

                return AjaxReloadPage;
            }

            return PartialView("_CreateEditTime", model);
        }

        [HttpGet]
        public ActionResult EditTime(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var time = work.ChurchEvents.GetTime(id);
            //time.StartDate = work.ChurchEvents.GetScheduler(time.ChurchEventSchedulerId).StartDate.ToShortDateString();
            //time.ShowMultiday = work.ChurchEvents.GetTimeBySchedulerId(time.ChurchEventSchedulerId).All(x => x.Id.Equals(time.Id));

            if (time.AllDay)
            {
                //time.StartTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour + 1, 0, 0).ToString("hh:mm tt");
                //time.EndTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour + 2, 0, 0).ToString("hh:mm tt");
                time.ShowEventAt = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, 0, 0).ToString("hh:mm tt");
                time.HideEventAt = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour + 2, 30, 0).ToString("hh:mm tt");
            }

            return PartialView("_CreateEditTime", time);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditTime(ChurchEventTime model)
        {
            if (ModelState.IsValid)
            {
                if (model.AllDay)
                {
                    //model.EndTime = model.ShowEventAt = model.HideEventAt = model.StartTime = null;
                }

                work.ChurchEvents.UpdateTime(model);

                return AjaxReloadPage;
            }

            return PartialView("_CreateEditTime", model);
        }

        public ActionResult DeleteTime(string id)
        {
            var time = work.ChurchEvents.GetTime(id);
            time.IsDeleted = true;
            work.ChurchEvents.UpdateTime(time);
            //var scheduler = work.ChurchEvents.GetScheduler(time.ChurchEventSchedulerId);
            CreateAlertMessage("The event time has been deleted.", AlertMessageTypes.Success, AlertMessageIcons.Success);

            //return RedirectToAction(nameof(Details), new { id = scheduler.EventId });
            return RedirectToAction(nameof(Details), new { id = 0 });
        }
        #endregion

        public ActionResult DuplicateEventTime(string eventId, string campusId)
        {
            var model = work.ChurchEvents.GetAllCampusTimeByEvent(eventId: eventId);
            model.CampusId = campusId;

            return PartialView("_DuplicateEventTime", model);
        }

        [HttpPost]
        public ActionResult DuplicateEventTime(string selectedId, string campusId, List<string> timesId)
        {
            var result = work.ChurchEvents.DuplicateEventTime(selectedId: selectedId, campusId: campusId, timesId: timesId);
            return Json(result);
        }

        public ActionResult CheckCampusTime(string campusId, string eventId)
        {
            var result = work.ChurchEvents.CheckCampusTime(campusId: campusId, eventId: eventId);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetScheduledCampuses(string eventId)
        {
            var result = work.ChurchEvents.GetScheduledCampuses(eventId: eventId);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult CheckIns()
        {
            var checkIns = work.ChurchEvents.GetAllCheckIns(SessionVariables.CurrentChurch.Id);
            var model = work.ChurchEvents.GetCheckInsModel(checkIns);
            return View(model);
        }

        public ActionResult FilterdCheckIns(IEnumerable<string> filter)
        {
            var checkIns = work.ChurchEvents.GetAllCheckIns(SessionVariables.CurrentChurch.Id, filter.IsNotNullOrEmpty() && filter.Any() ? filter.ToList() : null);
            var model = work.ChurchEvents.GetCheckInsModel(checkIns);
            return PartialView("_CheckinsList", model);
        }

        [HttpGet]
        public ActionResult _CheckInSomeone()
        {
            return PartialView("_AddCheckIn");
        }

        [RequirePermission(ModuleId = "7566091587eb1193b369cf404e9c32")]
        public ActionResult Labels()
        {
            return View();
        }

        [RequirePermission(ModuleId = "7747676769a0c86ce66e6045649ae0")]
        public ActionResult Stations()
        {
            return View();
        }
    }
}